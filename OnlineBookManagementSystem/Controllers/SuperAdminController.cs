using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class SuperAdminController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IBookService _bookService;
        private readonly IOrderService _orderService;
        private readonly IActivityLogger _activityLogger;
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public SuperAdminController(
            IUsersService usersService,
            IBookService bookService,
            IOrderService orderService,
            IActivityLogger activityLogger,
            ISystemSettingsService systemSettingsService,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _usersService = usersService;
            _bookService = bookService;
            _orderService = orderService;
            _activityLogger = activityLogger;
            _systemSettingsService = systemSettingsService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _usersService.GetSuperAdminDashboardDataAsync();
            await _activityLogger.LogAsync("Dashboard", "SuperAdmin dashboard accessed", userId);
            
            return View(viewModel);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ManageUsers(int page = 1, string? search = null, string? role = null, string? status = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _usersService.GetManageUsersDataAsync(page, 20, search, role, status);
            
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;
            
            await _activityLogger.LogAsync("ManageUsers", "User management page accessed", userId);
            return View(viewModel);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> SystemSettings()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _systemSettingsService.GetSystemSettingsAsync();
            await _activityLogger.LogAsync("SystemSettings", "System settings page accessed", userId);
            
            return View(viewModel);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ActivityLogs(int page = 1, string? search = null, string? action = null, string? role = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _activityLogger.GetActivityLogsAsync(page, 50, search, action, role, dateFrom, dateTo);
            
            ViewBag.Search = search;
            ViewBag.Action = action;
            ViewBag.Role = role;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _usersService.CreateUserAsync(request);
                if (result.Success)
                {
                    await _activityLogger.LogAsync("CreateUser", $"User '{request.Email}' created with role '{request.Role}'", userId);
                    return Json(new { success = true, message = "User created successfully" });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while creating the user" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UpdateGeneralSettings([FromForm] GeneralSettingsRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _systemSettingsService.UpdateGeneralSettingsAsync(request);
                if (result)
                {
                    await _activityLogger.LogAsync("UpdateSettings", "General settings updated", userId);
                    return Json(new { success = true, message = "Settings updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update settings" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating settings" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UpdateSecuritySettings([FromForm] SecuritySettingsRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _systemSettingsService.UpdateSecuritySettingsAsync(request);
                if (result)
                {
                    await _activityLogger.LogAsync("UpdateSettings", "Security settings updated", userId);
                    return Json(new { success = true, message = "Security settings updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update security settings" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating security settings" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UpdateEmailSettings([FromForm] EmailSettingsRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _systemSettingsService.UpdateEmailSettingsAsync(request);
                if (result)
                {
                    await _activityLogger.LogAsync("UpdateSettings", "Email settings updated", userId);
                    return Json(new { success = true, message = "Email settings updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update email settings" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating email settings" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> TestEmail()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _systemSettingsService.TestEmailConfigurationAsync();
                await _activityLogger.LogAsync("TestEmail", "Email configuration test performed", userId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to test email configuration" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ClearCache()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                await _systemSettingsService.ClearCacheAsync();
                await _activityLogger.LogAsync("ClearCache", "System cache cleared", userId);
                return Json(new { success = true, message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to clear cache" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> BackupDatabase()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _systemSettingsService.BackupDatabaseAsync();
                await _activityLogger.LogAsync("BackupDatabase", "Database backup initiated", userId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to backup database" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ClearOldLogs()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _activityLogger.ClearOldLogsAsync(30); // Clear logs older than 30 days
                await _activityLogger.LogAsync("ClearLogs", $"Cleared {result} old activity logs", userId);
                return Json(new { success = true, message = $"Cleared {result} old log entries" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to clear old logs" });
            }
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }

    // Request models for API endpoints
    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class GeneralSettingsRequest
    {
        public string SiteName { get; set; } = string.Empty;
        public string SiteDescription { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; }
    }

    public class SecuritySettingsRequest
    {
        public int JwtExpiry { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int LockoutDuration { get; set; }
        public bool RequireEmailConfirmation { get; set; }
    }

    public class EmailSettingsRequest
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}
