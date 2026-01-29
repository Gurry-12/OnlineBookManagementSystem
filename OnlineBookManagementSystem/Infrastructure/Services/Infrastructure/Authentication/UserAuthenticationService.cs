using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Core.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;


namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Authentication
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticationService> _logger;
        private readonly IActivityLogger _activityLogger;
        private readonly IEmailSender _emailSender;

        public UserAuthenticationService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<UserAuthenticationService> logger,
            IActivityLogger activityLogger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _activityLogger = activityLogger;
            _emailSender = emailSender;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Check if account is locked
                if (await IsAccountLockedAsync(user.Id))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Account is locked. Please try again later."
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    string message = result.IsLockedOut ? "Account is locked due to multiple failed attempts" :
                                   result.IsNotAllowed ? "Account is not allowed to sign in" :
                                   "Invalid email or password";

                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = message
                    };
                }

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = GenerateAccessToken(user, roles);
                var refreshTokenValue = GenerateRefreshToken();

                // Update user login info
                user.LastLoginDate = DateTime.UtcNow;

                // Add new refresh token
                var refreshToken = new RefreshToken(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(7));
                user.RefreshTokens.Add(refreshToken);

                await _userManager.UpdateAsync(user);

                await _activityLogger.LogAsync("UserLogin", $"User {user.Email} logged in successfully", user.Id);

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Authentication successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenValue,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes()),
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for email: {Email}", email);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "An error occurred during authentication"
                };
            }
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken && rt.IsActive()));

                if (user == null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = GenerateAccessToken(user, roles);
                var newRefreshTokenValue = GenerateRefreshToken();

                // Revoke old refresh token and add new one
                var oldToken = user.RefreshTokens.FirstOrDefault(rt => rt.IsActive());
                if (oldToken != null)
                {
                    oldToken.Revoke();
                }

                var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenValue, DateTime.UtcNow.AddDays(7));
                user.RefreshTokens.Add(newRefreshToken);

                await _userManager.UpdateAsync(user);

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshTokenValue,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes()),
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed for token: {RefreshToken}", refreshToken);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                // Find the user with the refresh token
                var user = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token && rt.IsActive()));

                if (user != null)
                {
                    var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == token && rt.IsActive());
                    if (refreshToken != null)
                    {
                        refreshToken.Revoke();
                        await _userManager.UpdateAsync(user);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token revocation failed for token: {Token}", token);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("PasswordChanged", "User changed password", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change failed for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    return (true, "If an account with that email exists, a password reset link has been sent.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7153";
                var resetLink = $"{baseUrl}/Auth/ResetPassword?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";

                var message = $@"
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password. Click the link below to reset it:</p>
                    <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Link expires in 1 hour.</p>";

                await _emailSender.SendEmailAsync(email, "Password Reset Request", message, $"Reset your password: {resetLink}");

                return (true, "If an account with that email exists, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for email: {Email}", email);
                return (false, "An error occurred while processing your request.");
            }
        }

        public async Task<(bool Success, string Message)> ConfirmPasswordResetAsync(string token, string email, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return (false, "Invalid request.");
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("PasswordReset", "User reset password via email", user.Id);
                    return (true, "Password has been reset successfully.");
                }

                return (false, "Failed to reset password. The token may be invalid or expired.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset confirmation failed for email: {Email}", email);
                return (false, "An error occurred while resetting your password.");
            }
        }

        public async Task<(bool Success, string Message)> SendEmailConfirmationAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return (false, "User not found.");
                }

                if (user.EmailConfirmed)
                {
                    return (false, "Email is already confirmed.");
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7153";
                var confirmationLink = $"{baseUrl}/Auth/ConfirmEmail?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(user.Email)}";

                var message = $@"
                    <h2>Email Confirmation</h2>
                    <p>Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>
                    <p>Link expires in 24 hours.</p>";

                await _emailSender.SendEmailAsync(user.Email, "Email Confirmation", message, $"Confirm your email: {confirmationLink}");

                return (true, "Confirmation email sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email confirmation send failed for user: {UserId}", userId);
                return (false, "An error occurred while sending confirmation email.");
            }
        }

        public async Task<(bool Success, string Message)> ConfirmEmailAsync(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return (false, "Invalid request.");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("EmailConfirmed", "User confirmed email address", user.Id);
                    return (true, "Email confirmed successfully.");
                }

                return (false, "Failed to confirm email. The token may be invalid or expired.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email confirmation failed for email: {Email}", email);
                return (false, "An error occurred while confirming your email.");
            }
        }

        public async Task<bool> LockAccountAsync(int userId, TimeSpan lockoutDuration)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(lockoutDuration));
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("AccountLocked", $"Account locked for {lockoutDuration.TotalMinutes} minutes", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account lock failed for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnlockAccountAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("AccountUnlocked", "Account unlocked", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account unlock failed for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsAccountLockedAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account lock check failed for user: {UserId}", userId);
                return false;
            }
        }

        private string GenerateAccessToken(User user, IList<string> roles)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)))),
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes()),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private int GetTokenExpiryMinutes()
        {
            return _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        }
    }
}