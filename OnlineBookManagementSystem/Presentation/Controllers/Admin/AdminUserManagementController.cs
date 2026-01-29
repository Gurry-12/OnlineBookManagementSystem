using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    /// <summary>
    /// Handles admin user management functionality following SRP.
    /// Responsible only for user administration tasks.
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    public class AdminUserManagementController : BaseController
    {
        private readonly IUsersService _userService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdminUserManagementController> _logger;

        public AdminUserManagementController(
            IUsersService userService,
            IActivityLogger activityLogger,
            ILogger<AdminUserManagementController> logger)
        {
            _userService = userService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> UserList(int page = 1, string? search = null, string? role = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _userService.GetUsersForAdminAsync(page, 20, search, role);

                ViewBag.Search = search;
                ViewBag.Role = role;

                await _activityLogger.LogAsync("ViewUsers", "Admin user list accessed", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user list for admin user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load user list.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int targetUserId, string role)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _userService.UpdateUserRoleAsync(targetUserId, role);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateUserRole", $"User {targetUserId} role changed to {role}", userId);
                    return Json(new { success = true, message = "User role updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update user role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {TargetUserId} by admin {UserId}", targetUserId, userId);
                return Json(new { success = false, message = "An error occurred while updating user role" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int targetUserId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _userService.ToggleUserStatusAsync(targetUserId, true);
                if (success)
                {
                    await _activityLogger.LogAsync("ToggleUserStatus", $"User {targetUserId} status toggled", userId);
                    return Json(new { success = true, message = "User status updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update user status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {TargetUserId} by admin {UserId}", targetUserId, userId);
                return Json(new { success = false, message = "An error occurred while updating user status" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int targetUserId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            // Prevent self-deletion
            if (userId == targetUserId)
            {
                return Json(new { success = false, message = "Cannot delete your own account" });
            }

            try
            {
                var success = await _userService.SoftDeleteUserAsync(targetUserId);
                if (success)
                {
                    await _activityLogger.LogAsync("DeleteUser", $"User {targetUserId} deleted", userId);
                    return Json(new { success = true, message = "User deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete user or user not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {TargetUserId} by admin {UserId}", targetUserId, userId);
                return Json(new { success = false, message = "An error occurred while deleting user" });
            }
        }

        public async Task<IActionResult> UserDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var user = await _userService.GetUserDetailsAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(UserList));
                }

                await _activityLogger.LogAsync("ViewUserDetails", $"Admin viewed details for user {id}", userId);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for user {TargetUserId}", id);
                TempData["ErrorMessage"] = "Failed to load user details.";
                return RedirectToAction(nameof(UserList));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var stats = await _userService.GetUserStatisticsAsync(userId);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user statistics");
                return Json(new { success = false, message = "Failed to load user statistics" });
            }
        }
    }
}