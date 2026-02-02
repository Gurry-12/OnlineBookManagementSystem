using Microsoft.AspNetCore.Identity;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Users
{
    public class UserCommandService : IUserCommandService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<UserCommandService> _logger;
        private readonly IActivityLogger _activityLogger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public UserCommandService(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<UserCommandService> logger,
            IActivityLogger activityLogger,
            IEmailSender emailSender,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _activityLogger = activityLogger;
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return (false, "User with this email already exists");
                }

                // Validate role
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    return (false, "Invalid role specified");
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Name = request.Name,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, request.Role);

                _logger.LogInformation("User created: {Email} with role {Role}", request.Email, request.Role);
                return (true, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", request.Email);
                return (false, "An error occurred while creating the user");
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || (bool)user.IsDeleted)
                    return false;

                if (!await _roleManager.RoleExistsAsync(newRole))
                    return false;

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                _logger.LogInformation("User role updated: {UserId} to {Role}", userId, newRole);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user role: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                if (isActive)
                {
                    // Activate user
                    user.LockoutEnd = null;
                    user.IsDeleted = false;
                }
                else
                {
                    // Deactivate user
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle user status: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                user.IsDeleted = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Also lock the account

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("SoftDeleteUser", $"User {user.Email} soft deleted", userId);
                    _logger.LogInformation("User soft deleted: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to soft delete user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, object profileModel)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || (bool)user.IsDeleted)
                    return false;

                // Use reflection to update user properties from the profile model
                var profileType = profileModel.GetType();
                var userType = typeof(User);

                // Map common properties
                var nameProperty = profileType.GetProperty("Name");
                if (nameProperty != null)
                {
                    var nameValue = nameProperty.GetValue(profileModel)?.ToString();
                    if (!string.IsNullOrEmpty(nameValue))
                    {
                        user.Name = nameValue;
                    }
                }

                var emailProperty = profileType.GetProperty("Email");
                if (emailProperty != null)
                {
                    var emailValue = emailProperty.GetValue(profileModel)?.ToString();
                    if (!string.IsNullOrEmpty(emailValue) && emailValue != user.Email)
                    {
                        user.Email = emailValue;
                        user.UserName = emailValue;
                        user.EmailConfirmed = false; // Require re-confirmation for email changes
                    }
                }

                // Map address properties if they exist
                var addressProperty = profileType.GetProperty("Address");
                if (addressProperty != null)
                {
                    user.Address = addressProperty.GetValue(profileModel)?.ToString();
                }

                var cityProperty = profileType.GetProperty("City");
                if (cityProperty != null)
                {
                    user.City = cityProperty.GetValue(profileModel)?.ToString();
                }

                var stateProperty = profileType.GetProperty("State");
                if (stateProperty != null)
                {
                    user.State = stateProperty.GetValue(profileModel)?.ToString();
                }

                var countryProperty = profileType.GetProperty("Country");
                if (countryProperty != null)
                {
                    user.Country = countryProperty.GetValue(profileModel)?.ToString();
                }

                var zipCodeProperty = profileType.GetProperty("ZipCode");
                if (zipCodeProperty != null)
                {
                    user.ZipCode = zipCodeProperty.GetValue(profileModel)?.ToString();
                }

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _activityLogger.LogAsync("UpdateProfile", "User updated profile information", userId);
                    _logger.LogInformation("User profile updated: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user profile: {UserId}", userId);
                return false;
            }
        }

        public async Task<(bool Success, string Message)> ApproveUserAsync(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, "User not found");

            if (!await _roleManager.RoleExistsAsync(role)) return (false, "Invalid role");

            // 1. Generate Custom Confirmation Token
            var rawToken = Guid.NewGuid().ToString("N");

            // 2. Hash it for storage
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var hashedToken = Convert.ToBase64String(bytes);

            // 3. Update User
            user.IsPendingApproval = false;
            user.EmailConfirmationToken = hashedToken;
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.EmailConfirmed = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (false, "Failed to update user");

            // 4. Assign Role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            // 5. Send Email
            var baseUrl = _config["AppUrl"] ?? "https://localhost:7153"; // Fallback dev url
            var confirmationLink = $"{baseUrl}/Auth/ConfirmEmail?token={WebUtility.UrlEncode(rawToken)}&email={WebUtility.UrlEncode(user.Email)}";

            var message = $@"
                <h2>Welcome to Whispering Pages!</h2>
                <p>Your account has been approved by the administrator.</p>
                <p>Please confirm your email address to activate your account and login:</p>
                <p><a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>
                <p>Link expires in 24 hours.</p>";

            await _emailSender.SendEmailAsync(user.Email, "Account Approved - Confirm Email", message, $"Your account is approved. Confirm email: {confirmationLink}");

            await _activityLogger.LogAsync("ApproveUser", $"User {user.Email} approved as {role}. Confirmation email sent.", 0);

            return (true, "User approved & confirmation email sent.");
        }

        public async Task<(bool Success, string Message)> RejectUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, "User not found");

            // Soft delete
            user.IsDeleted = true;
            user.IsPendingApproval = false; // clear pending

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (false, "Failed to reject user");

            // Send email notification (placeholder)

            await _activityLogger.LogAsync("RejectUser", $"User {user.Email} rejected", 0);

            return (true, "User rejected successfully");
        }
    }
}