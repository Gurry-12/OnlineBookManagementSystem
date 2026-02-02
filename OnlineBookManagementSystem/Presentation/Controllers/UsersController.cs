using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.Users;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUsersService usersService,
            IActivityLogger activityLogger,
            ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UserManagement(int page = 1, string? search = null, string? role = null, string? status = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Determine if user is SuperAdmin
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            if (isSuperAdmin)
            {
                // Redirect to SuperAdmin controller
                return RedirectToAction("ManageUsers", "SuperAdmin", new { page, search, role, status });
            }
            else
            {
                // Redirect to Admin controller
                return RedirectToAction("UserList", "Admin", new { page, search, role, status });
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UserDetails(int id, bool edit = false)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var userDetails = await _usersService.GetUserDetailsAsync(id);
                if (userDetails == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserManagement");
                }

                // Determine capabilities based on current user role
                var isSuperAdmin = User.IsInRole("SuperAdmin");
                var isAdmin = User.IsInRole("Admin");

                var viewModel = new UserDetailViewModel
                {
                    User = userDetails,
                    IsEditMode = edit,
                    Capabilities = new UserManagementCapabilities
                    {
                        CanView = true,
                        CanEdit = isSuperAdmin || isAdmin,
                        CanDelete = isSuperAdmin,
                        CanChangeRoles = isSuperAdmin,
                        CanLockUnlock = isSuperAdmin,
                        CanViewSensitiveData = isSuperAdmin || isAdmin,
                        CanViewAllUsers = isSuperAdmin,
                        CanManageSuperAdmins = isSuperAdmin
                    }
                };

                // Set appropriate layout
                ViewData["Layout"] = isSuperAdmin ? "_LayoutSuperAdmin" : "_LayoutAdmin";

                await _activityLogger.LogAsync("ViewUserDetails", $"Viewed details for user {id}", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving user details.";
                return RedirectToAction("UserManagement");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool success = true;
                var messages = new List<string>();

                // Update user role if provided and user has permission
                if (!string.IsNullOrEmpty(request.Role) && User.IsInRole("SuperAdmin"))
                {
                    var roleResult = await _usersService.UpdateUserRoleAsync(id, request.Role);
                    if (roleResult)
                    {
                        messages.Add("Role updated successfully");
                    }
                    else
                    {
                        success = false;
                        messages.Add("Failed to update role");
                    }
                }
                else if (!string.IsNullOrEmpty(request.Role) && !User.IsInRole("SuperAdmin"))
                {
                    return Json(new { success = false, message = "Only SuperAdmin can change user roles" });
                }

                // Update user status if provided
                if (request.IsActive.HasValue)
                {
                    var statusResult = await _usersService.ToggleUserStatusAsync(id, request.IsActive.Value);
                    if (statusResult)
                    {
                        messages.Add("Status updated successfully");
                    }
                    else
                    {
                        success = false;
                        messages.Add("Failed to update status");
                    }
                }

                if (success)
                {
                    await _activityLogger.LogAsync("UpdateUser", $"Updated user {id}", userId);
                    return Json(new { success = true, message = string.Join(", ", messages) });
                }
                else
                {
                    return Json(new { success = false, message = string.Join(", ", messages) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return Json(new { success = false, message = "An error occurred while updating the user" });
            }
        }
    }

    // Request model for updating users
    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsActive { get; set; }
    }
}
