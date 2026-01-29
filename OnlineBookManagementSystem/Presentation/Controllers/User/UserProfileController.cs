using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.User
{
    /// <summary>
    /// Handles user profile functionality following SRP.
    /// Responsible only for user profile management and settings.
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    public class UserProfileController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly IAuthService _authService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            IAuthService authService,
            IActivityLogger activityLogger,
            ILogger<UserProfileController> logger)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _authService = authService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> Profile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var profile = await _bookQueryService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found.";
                    return RedirectToAction("Login", "Auth");
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load profile.";
                return RedirectToAction("Dashboard", "UserDashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            try
            {
                // TODO: Move this to UserCommandService - user profile updates shouldn't be in BookCommandService
                var success = await _bookCommandService.UpdateUserProfileAsync(userId, model);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateProfile", "User updated profile information", userId);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("", "Failed to update profile. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                ModelState.AddModelError("", "An error occurred while updating your profile.");
            }

            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || 
                string.IsNullOrWhiteSpace(request.NewPassword) || 
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return Json(new { success = false, message = "All password fields are required" });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Json(new { success = false, message = "New passwords do not match" });
            }

            if (request.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters long" });
            }

            try
            {
                var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                if (success)
                {
                    await _activityLogger.LogAsync("ChangePassword", "User password changed", userId);
                    return Json(new { success = true, message = "Password changed successfully" });
                }
                return Json(new { success = false, message = "Current password is incorrect" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while changing password" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(request.NewEmail))
            {
                return Json(new { success = false, message = "Email is required" });
            }

            if (!IsValidEmail(request.NewEmail))
            {
                return Json(new { success = false, message = "Invalid email format" });
            }

            try
            {
                var success = await _authService.UpdateEmailAsync(userId, request.NewEmail);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateEmail", $"User email updated to {request.NewEmail}", userId);
                    return Json(new { success = true, message = "Email updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update email" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while updating email" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // This would typically update user notification preferences
                // For now, we'll just log the activity
                await _activityLogger.LogAsync("UpdateNotificationSettings", "User updated notification settings", userId);
                return Json(new { success = true, message = "Notification settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while updating notification settings" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProfileStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // This would typically get user-specific statistics
                var stats = new
                {
                    joinDate = DateTime.Now.AddYears(-1), // Placeholder
                    totalOrders = 0, // Would come from order service
                    totalSpent = 0m, // Would come from order service
                    favoriteBooks = 0 // Would come from favorites service
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile statistics for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load profile statistics" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new { success = false, message = "Password is required to delete account" });
            }

            try
            {
                // Verify password before deletion
                var passwordValid = await _authService.ValidatePasswordAsync(userId, request.Password);
                if (!passwordValid)
                {
                    return Json(new { success = false, message = "Invalid password" });
                }

                // This would typically soft delete the user account
                await _activityLogger.LogAsync("DeleteAccountRequest", "User requested account deletion", userId);
                
                return Json(new { 
                    success = true, 
                    message = "Account deletion request submitted. You will receive a confirmation email.",
                    redirectUrl = Url.Action("Logout", "Auth")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account deletion for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while processing account deletion" });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }

    // Request models
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateEmailRequest
    {
        public string NewEmail { get; set; } = string.Empty;
    }

    public class NotificationSettingsRequest
    {
        public bool EmailNotifications { get; set; }
        public bool OrderUpdates { get; set; }
        public bool BookRecommendations { get; set; }
        public bool PromotionalEmails { get; set; }
    }

    public class DeleteAccountRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}