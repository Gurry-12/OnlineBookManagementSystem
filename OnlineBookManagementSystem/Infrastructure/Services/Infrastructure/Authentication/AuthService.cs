using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Fixed: Changed from System.Data.Entity
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Services.Helpers;
using OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private const int Minutes = 30; // Increased to 60 for easier testing
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly BookManagementContext _context;
        private readonly IConfiguration _config;
        private readonly IDnsChecker _dnsChecker;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<User> userManager,

            RoleManager<IdentityRole<int>> roleManager,
            BookManagementContext context,
            IConfiguration config,
            IDnsChecker dnsChecker,
            IMemoryCache cache,
            ILogger<AuthService> logger,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
            _dnsChecker = dnsChecker;
            _cache = cache;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SeedRolesAsync()
        {
            var roles = new[] { "SuperAdmin", "Admin", "User", "Guest" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                    _logger.LogInformation("Seeded role: {Role}", role);
                }
            }
        }

        public async Task<(bool Success, string Message, User? User)> ValidateUserAsync(LoginViewModel data)
        {
            try
            {
                // Input validation
                if (data == null || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
                {
                    _logger.LogWarning("Login attempt with invalid input data");
                    return (false, "Invalid input.", null);
                }

                var user = await _userManager.FindByEmailAsync(data.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent email: {Email}", data.Email);
                    return (false, "Invalid credentials.", null);
                }

                // Check if user is deleted
                if (user.IsDeleted == true)
                {
                    _logger.LogWarning("Login attempt for deleted user: {Email}", data.Email);
                    return (false, "Account no longer available.", null);
                }

                // Check password
                if (!await _userManager.CheckPasswordAsync(user, data.Password))
                {
                    await _userManager.AccessFailedAsync(user);
                    _logger.LogWarning("Failed password attempt for {Email}", data.Email);
                    return (false, "Invalid credentials.", null);
                }

                // Check if account is pending approval
                if (user.IsPendingApproval)
                {
                    _logger.LogWarning("Login attempt for pending user {Email}", data.Email);
                    return (false, "Account under review. Please wait for approval.", null);
                }

                // Check email confirmation
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login attempt for unconfirmed email: {Email}", data.Email);
                    return (false, "Please confirm your email first. Check your inbox.", null);
                }

                // Check if account is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    _logger.LogWarning("Login attempt for locked out user {Email}, lockout ends: {LockoutEnd}", data.Email, lockoutEnd);
                    return (false, $"Account is locked. Try again after {lockoutEnd?.ToString("yyyy-MM-dd HH:mm")}.", null);
                }

                // Reset failed access count on successful validation
                await _userManager.ResetAccessFailedCountAsync(user);

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in successfully.", data.Email);
                return (true, "Login successful.", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user validation for email: {Email}", data?.Email ?? "Unknown");
                return (false, "Authentication service temporarily unavailable. Please try again.", null);
            }
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogError("Attempted to generate tokens for null user");
                    throw new ArgumentNullException(nameof(user));
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email ?? ""),
                    new(ClaimTypes.Name, user.Name ?? "")
                };

                // Fix: Use proper async/await instead of .Result
                var roles = await _userManager.GetRolesAsync(user);
                if (roles?.Any() == true)
                {
                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                }
                else
                {
                    _logger.LogWarning("User {UserId} has no roles assigned", user.Id);
                }

                var jwtKey = _config["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Key is not configured");
                    throw new InvalidOperationException("JWT configuration is missing");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiryMinutes = _config.GetValue<int>("Jwt:ExpiryMinutes", Minutes);
                var accessToken = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: creds);

                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

                var hashedRefresh = HashToken(refreshToken);

                var tokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = hashedRefresh,
                    ExpiryDate = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7)),
                    CreatedByIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

                _context.RefreshTokens.Add(tokenEntity);

                // Handle concurrency conflicts with retry logic
                var maxRetries = 3;
                var retryCount = 0;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        break; // Success, exit retry loop
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        retryCount++;
                        _logger.LogWarning("Concurrency conflict occurred while saving refresh token. Retry {RetryCount}/{MaxRetries}", retryCount, maxRetries);

                        if (retryCount >= maxRetries)
                        {
                            _logger.LogError(ex, "Failed to save refresh token after {MaxRetries} retries due to concurrency conflicts", maxRetries);
                            throw;
                        }

                        // Refresh the entity from database and retry
                        foreach (var entry in ex.Entries)
                        {
                            await entry.ReloadAsync();
                        }

                        // Wait a bit before retrying
                        await Task.Delay(100 * retryCount);
                    }
                }

                _logger.LogInformation("Generated tokens for user {UserId}", user.Id);
                return (accessTokenString, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {UserId}", user?.Id ?? 0);
                throw;
            }
        }

        // Keep the synchronous version for backward compatibility but mark as obsolete


        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        public async Task<(bool Success, string Message, string? ConfirmationToken)> RegisterUserAsync(RegisterViewModel data)
        {
            if (await _userManager.FindByEmailAsync(data.Email) != null)
                return (false, "Email already registered.", null);

            var user = new User
            {
                UserName = data.Email,
                Email = data.Email,
                Name = data.Name,
                IsPendingApproval = true,
                RequestDate = DateTime.UtcNow,
                RequestedRole = data.RequestedRole,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, data.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            // No role assigned yet - waiting for approval
            // await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User {Email} registered (pending approval).", data.Email);
            return (true, "Registration successful. Your account is pending approval.", null);
        }

        public async Task<bool> ConfirmEmailAsync(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Custom Token Verification
            if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow) return false;

            var hashedToken = HashToken(token);
            if (user.EmailConfirmationToken != hashedToken) return false;

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            // Rate Limiting Logic
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var emailKey = $"ResetLimit_Email_{email}";
            var ipKey = $"ResetLimit_IP_{ip}";

            var emailCount = _cache.GetOrCreate(emailKey, entry => { entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); return 0; });
            var ipCount = _cache.GetOrCreate(ipKey, entry => { entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); return 0; });

            if (emailCount >= 5 || ipCount >= 5)
            {
                _logger.LogWarning("Rate limit exceeded for password reset. Email: {Email}, IP: {IP}", email, ip);
                return null;
            }

            // Increment counters
            _cache.Set(emailKey, emailCount + 1, TimeSpan.FromHours(1));
            _cache.Set(ipKey, ipCount + 1, TimeSpan.FromHours(1));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || (bool)user.IsDeleted) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.PasswordResetToken = HashToken(token);
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(30); // 30 min expiration
            await _userManager.UpdateAsync(user);

            // Construct secure link (requires HttpContext for base URL, or config)
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : _config["AppUrl"];
            var resetLink = $"{baseUrl}/Auth/ResetPassword?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";

            var message = $@"
                <h2>Password Reset Request</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password. Click the link below to proceed:</p>
                <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Whispering Pages Team</p>";

            await _emailSender.SendEmailAsync(email, "Reset Your Password", message, $"Please reset your password by copying this link: {resetLink}");

            return token;
        }

        // Example: Sending Confirmation Email (Feature #2 prep)
        public async Task SendWelcomeEmailAsync(User user)
        {
            var subject = "Welcome to Whispering Pages!";
            var message = $@"
                <h1>Welcome, {user.Name}!</h1>
                <p>Thank you for registering. Your account is currently under review by our administrators.</p>
                <p>You will receive another email once your account is approved.</p>";

            await _emailSender.SendEmailAsync(user.Email, subject, message);
        }

        public async Task SendUserApprovedEmailAsync(User user, string confirmationLink)
        {
            var message = $@"
                <h2>Welcome to Whispering Pages!</h2>
                <p>Your account has been approved by the administrator.</p>
                <p>Please confirm your email address to activate your account and login:</p>
                <p><a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>
                <p>Link expires in 24 hours.</p>";

            await _emailSender.SendEmailAsync(user.Email, "Account Approved - Confirm Email", message, $"Your account is approved. Confirm email: {confirmationLink}");
        }

        public async Task<bool> UpdatePasswordAsync(string token, string newPassword)
        {
            var hashedToken = HashToken(token);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == hashedToken && u.PasswordResetExpiry > DateTime.UtcNow);
            if (user == null) return false;

            // We use the original token for Identity ResetPasswordAsync because Identity validates its own token signature
            // But here we are using a custom token flow stored in User entity for the LINK verification.
            // Wait, GeneratePasswordResetTokenAsync uses _userManager.GeneratePasswordResetTokenAsync.
            // That token is valid for Identity.
            // We stored the HASH of it.
            // If we found the user by hash, we know it's the right user and token.

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<UserViewModel?> GetUserProfileAsync(int userId)
        {
            if (!_cache.TryGetValue($"user_{userId}", out UserViewModel? profile))
            {
                // FIX: Replace 'and' with '&&' for logical AND, and ensure correct lambda syntax
                var user = await _context.Users
                    .Include(u => u.ShoppingCarts) // Ensure ShoppingCarts are loaded
                    .FirstOrDefaultAsync(u => u.Id == userId && (u.IsDeleted == null || (bool)!u.IsDeleted));

                if (user == null) return null;

                var roles = await _userManager.GetRolesAsync(user);

                profile = new UserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = string.Join(", ", roles),
                    CartItemCount = user.ShoppingCarts.Where(sc => !sc.IsDeleted).Sum(sc => sc.Quantity)
                };

                _cache.Set($"user_{userId}", profile, TimeSpan.FromMinutes(5));
            }
            return profile;
        }

        public User GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && (bool)!u.IsDeleted);
            return user ?? throw new InvalidOperationException("User not found.");
        }

        public async Task<bool> UpdateUserDetailAsync(ProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null || (bool)user.IsDeleted) return false;

            // DEV FIX: Commented out DNS check
            // if (!await _dnsChecker.DomainHasMxRecordAsync(model.NewEmail)) return false;

            var oldEmail = user.Email;
            user.Name = model.NewName;
            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _cache.Remove($"user_{model.Id}");
                return true;
            }
            return false;
        }

        public void UpdateUserDetailAsync(User user)
        {
            throw new NotImplementedException("Use async version.");
        }

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !await _roleManager.RoleExistsAsync(roleName)) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (roleName == "SuperAdmin" && !currentRoles.Contains("SuperAdmin"))
            {
                _logger.LogWarning("Unauthorized role assignment");
                return false;
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, roleName);
            _cache.Remove($"user_{userId}");
            return true;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null ? (List<string>)await _userManager.GetRolesAsync(user) : new List<string>();
        }

        public async Task RevokeRefreshTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsDeleted)
                .ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.ReplacedByToken = "LOGOUT";
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string AccessToken, string RefreshToken, string Message)> RefreshTokenAsync(string token)
        {
            var hashedToken = HashToken(token);
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .Where(rt => !rt.IsDeleted) // Add explicit filter since we removed global query filter
                .SingleOrDefaultAsync(rt => rt.Token == hashedToken);

            if (existingToken == null)
                return (false, "", "", "Invalid token.");

            if (existingToken.IsRevoked || existingToken.ExpiryDate < DateTime.UtcNow)
                return (false, "", "", "Token expired or revoked.");

            var user = existingToken.User;
            if (user == null || (bool)user.IsDeleted)
                return (false, "", "", "User not valid.");

            // Revoke current token
            existingToken.IsRevoked = true;

            // Fix: Use async GenerateTokensAsync method
            var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user);

            existingToken.ReplacedByToken = HashToken(newRefreshToken); // Store hash of new token

            // Handle concurrency conflicts with retry logic
            var maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    break; // Success, exit retry loop
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning("Concurrency conflict occurred while updating refresh token. Retry {RetryCount}/{MaxRetries}", retryCount, maxRetries);

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "Failed to update refresh token after {MaxRetries} retries due to concurrency conflicts", maxRetries);
                        throw;
                    }

                    // Refresh the entity from database and retry
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync();
                        // Re-apply the changes after reload
                        if (entry.Entity is RefreshToken refreshToken)
                        {
                            refreshToken.IsRevoked = true;
                            refreshToken.ReplacedByToken = HashToken(newRefreshToken);
                        }
                    }

                    // Wait a bit before retrying
                    await Task.Delay(100 * retryCount);
                }
            }

            return (true, newAccessToken, newRefreshToken, "Token refreshed.");
        }

        // Optimized ManageUsers (Fixes N+1 problem)
        public async Task<List<UserViewModel>> ManageUsers()
        {
            // Fetch users + roles in one go is hard with Identity, 
            // but we can at least fetch all users first.
            var users = await _context.Users
                .Where(u => (bool)!u.IsDeleted)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                // Identity stores roles in a separate table, so we must query for each 
                // OR join manually. For now, querying per user is standard Identity usage,
                // but we await it properly.
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = string.Join(", ", roles) // Assuming View expects string, not List
                });
            }

            return userViewModels;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || (bool)user.IsDeleted) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                _cache.Remove($"user_{userId}");
                return true;
            }
            return false;
        }

        // Additional methods for user profile management
        public async Task<bool> UpdateEmailAsync(int userId, string newEmail)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || (bool)user.IsDeleted) return false;

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

                if (result.Succeeded)
                {
                    user.UserName = newEmail; // Update username to match email
                    await _userManager.UpdateAsync(user);
                    _cache.Remove($"user_{userId}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ValidatePasswordAsync(int userId, string password)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || (bool)user.IsDeleted) return false;

                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user {UserId}", userId);
                return false;
            }
        }
    }
}
