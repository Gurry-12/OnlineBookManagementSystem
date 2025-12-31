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
        public async Task<IActionResult> PendingUsers()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var pendingUsers = await _usersService.GetPendingUsersAsync();

            await _activityLogger.LogAsync("PendingUsers", "Pending users page accessed", userId);
            return View(pendingUsers);
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ApproveUser(int userId, string role)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            var result = await _usersService.ApproveUserAsync(userId, role);
            if (result.Success)
            {
                await _activityLogger.LogAsync("ApproveUser", $"Approved user {userId} as {role}", adminId);
            }
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> RejectUser(int userId)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            var result = await _usersService.RejectUserAsync(userId);
            if (result.Success)
            {
                await _activityLogger.LogAsync("RejectUser", $"Rejected user {userId}", adminId);
            }
            return Json(new { success = result.Success, message = result.Message });
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
        public async Task<IActionResult> ActivityLogs(int page = 1, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _activityLogger.GetActivityLogsAsync(page, 50, search, dateFrom, dateTo);

            ViewBag.Search = search;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(viewModel);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        public async Task<IActionResult> ExportActivityLogs(string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logs = await _activityLogger.GetActivityLogsAsync(1, 100000, search, dateFrom, dateTo); // Get all matching

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,Action Type,Description,User,IP Address,User Agent");

            foreach (var log in logs.Logs)
            {
                var user = log.User?.Email ?? "System";
                var desc = log.Description?.Replace(",", ";") ?? "";
                csv.AppendLine($"{log.Timestamp},{log.ActionType},{desc},{user},{log.IpAddress},{log.UserAgent}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"ActivityLogs_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to clear old logs" });
            }
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult SwitchToRole(string role)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Store the original role and current switched role in session
            if (HttpContext.Session.GetString("OriginalRole") == null)
            {
                HttpContext.Session.SetString("OriginalRole", "SuperAdmin");
            }
            
            HttpContext.Session.SetString("CurrentViewRole", role);

            // Redirect to the appropriate dashboard based on role
            return role.ToLower() switch
            {
                "admin" => RedirectToAction("Dashboard", "Admin"),
                "user" => RedirectToAction("Dashboard", "User"),
                "public" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Dashboard", "SuperAdmin")
            };
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult ReturnToSuperAdmin()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Clear the switched role session
            HttpContext.Session.Remove("CurrentViewRole");
            HttpContext.Session.Remove("OriginalRole");

            return RedirectToAction("Dashboard", "SuperAdmin");
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


}
