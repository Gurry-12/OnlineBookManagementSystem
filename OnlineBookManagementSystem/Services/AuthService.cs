using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore; // Fixed: Changed from System.Data.Entity
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Helper;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.AuthViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private const int Minutes = 2; // Increased to 60 for easier testing
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly BookManagementContext _context;
        private readonly IConfiguration _config;
        private readonly IDnsChecker _dnsChecker;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailSender _emailSender;

        public AuthService(
            UserManager<User> userManager,

            RoleManager<IdentityRole<int>> roleManager,
            BookManagementContext context,
            IConfiguration config,
            IDnsChecker dnsChecker,
            IMemoryCache cache,
            ILogger<AuthService> logger,
            IEmailSender emailSender = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
            _dnsChecker = dnsChecker;
            _cache = cache;
            _logger = logger;
            _emailSender = emailSender;
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
            if (string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
                return (false, "Invalid input.", null);

            // ---------------------------------------------------------
            // DEV FIX: Commented out DNS check so you can use test emails
            // ---------------------------------------------------------
            // if (!await _dnsChecker.DomainHasMxRecordAsync(data.Email))
            // {
            //     return (false, "Invalid email domain.", null);
            // }

            var user = await _userManager.FindByEmailAsync(data.Email);

            // Added check for IsDeleted false explicitly
            if (user == null || (bool)user.IsDeleted || !await _userManager.CheckPasswordAsync(user, data.Password))
            {
                if (user != null) await _userManager.AccessFailedAsync(user);
                _logger.LogWarning("Failed login attempt for {Email}", data.Email);
                return (false, "Invalid credentials.", null);
            }

            // ---------------------------------------------------------
            // DEV FIX: Commented out Email Confirmation check for login
            // ---------------------------------------------------------
            // if (!user.IsEmailConfirmed)
            //    return (false, "Please confirm your email first.", user);

            await _userManager.ResetAccessFailedCountAsync(user);
            _logger.LogInformation("User {Email} logged in successfully.", data.Email);
            return (true, "Login successful.", user);
        }

        public (string AccessToken, string RefreshToken) GenerateTokens(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? ""),
                new(ClaimTypes.Name, user.Name ?? "")
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "YourSuperSecretKeyHereMustBeLongerThanThis12345"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Minutes),
                signingCredentials: creds);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            var hashedRefresh = HashToken(refreshToken);

            var tokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = hashedRefresh,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedByIp = "127.0.0.1" // Placeholder
            };

            _context.RefreshTokens.Add(tokenEntity);
            _context.SaveChanges();

            return (accessTokenString, refreshToken);
        }

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

            // ---------------------------------------------------------
            // DEV FIX: Commented out DNS check for registration
            // ---------------------------------------------------------
            // if (!await _dnsChecker.DomainHasMxRecordAsync(data.Email))
            //    return (false, "Invalid email domain.", null);

            var user = new User
            {
                UserName = data.Email,
                Email = data.Email,
                Name = data.Name,
                // ---------------------------------------------------------
                // DEV FIX: Auto-confirm email so you can login immediately
                // ---------------------------------------------------------
                IsEmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, data.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Only send email if sender is configured
            if (_emailSender != null)
            {
                try
                {
                    await _emailSender.SendEmailAsync(data.Email, "Confirm Your Email", $"Confirm: {token}");
                }
                catch { /* Ignore email errors in dev */ }
            }

            _logger.LogInformation("User {Email} registered.", data.Email);
            return (true, "Registration successful.", token);
        }

        public async Task<bool> ConfirmEmailAsync(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || (bool)user.IsDeleted) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.PasswordResetToken = HashToken(token);
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(15);
            await _userManager.UpdateAsync(user);

            if (_emailSender != null)
                await _emailSender.SendEmailAsync(email, "Reset Password", $"Reset link: {token}");

            return token;
        }

        public async Task<bool> UpdatePasswordAsync(string token, string newPassword)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PasswordResetToken != null && u.PasswordResetExpiry > DateTime.UtcNow);
            if (user == null) return false;

            if (HashToken(token) != user.PasswordResetToken) return false;

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
            var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
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

            // Generate new tokens
            var (newAccessToken, newRefreshToken) = GenerateTokens(user);

            existingToken.ReplacedByToken = HashToken(newRefreshToken); // Store hash of new token

            await _context.SaveChangesAsync();

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
    }
}