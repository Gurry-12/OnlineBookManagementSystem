using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class SuperAdminController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly UserManager<OnlineBookManagementSystem.Core.Domain.Entities.User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            IUsersService usersService,
            IOrderQueryService orderQueryService,
            IActivityLogger activityLogger,
            ISystemSettingsService systemSettingsService,
            UserManager<OnlineBookManagementSystem.Core.Domain.Entities.User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<SuperAdminController> logger)
        {
            _usersService = usersService;
            _orderQueryService = orderQueryService;
            _activityLogger = activityLogger;
            _systemSettingsService = systemSettingsService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var dashboardData = await _usersService.GetSuperAdminDashboardDataAsync();
            var systemSettings = await _systemSettingsService.GetSystemSettingsAsync();
            var pendingUsers = await _usersService.GetPendingUsersAsync();

            // Enhanced dashboard with consolidated information
            var enhancedViewModel = new EnhancedSuperAdminDashboardViewModel
            {
                // Original dashboard data
                TotalUsers = dashboardData.TotalUsers,
                NewUsersToday = dashboardData.NewUsersToday,
                TotalBooks = dashboardData.TotalBooks,
                BooksAddedThisMonth = dashboardData.BooksAddedThisMonth,
                TotalOrders = dashboardData.TotalOrders,
                OrdersToday = dashboardData.OrdersToday,
                TotalRevenue = dashboardData.TotalRevenue,
                RevenueToday = dashboardData.RevenueToday,
                StorageUsagePercent = dashboardData.StorageUsagePercent,
                ActiveSessions = dashboardData.ActiveSessions,
                RecentActivities = dashboardData.RecentActivities,

                // Enhanced information
                SystemInfo = new SystemInfoSummary
                {
                    AppVersion = systemSettings.AppVersion,
                    Environment = systemSettings.Environment,
                    MaintenanceMode = systemSettings.MaintenanceMode,
                    ServerUptime = systemSettings.ServerUptime
                },
                PendingUsersCount = pendingUsers.Count,
                PendingUsers = pendingUsers.Take(5).ToList(),

                // Quick actions
                QuickActions = new List<QuickAction>
                {
                    new() { Title = "Manage Users", Icon = "bi-people", Action = "ManageUsers", Description = "User & role management" },
                    new() { Title = "System Settings", Icon = "bi-sliders2", Action = "SystemSettings", Description = "Configure system" },
                    new() { Title = "Activity Logs", Icon = "bi-journal-text", Action = "ActivityLogs", Description = "View system logs" },
                    new() { Title = "Clear Cache", Icon = "bi-arrow-clockwise", Action = "javascript:clearCache()", Description = "Clear system cache" },
                    new() { Title = "Backup Database", Icon = "bi-download", Action = "javascript:backupDatabase()", Description = "Create backup" }
                }
            };

            await _activityLogger.LogAsync("Dashboard", "Enhanced SuperAdmin dashboard accessed", userId);
            return View(enhancedViewModel);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ManageUsers(int page = 1, string? search = null, string? role = null, string? status = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _usersService.GetManageUsersDataAsync(page, 20, search, role, status);

            // Convert to unified ViewModel with SuperAdmin capabilities
            var unifiedViewModel = new OnlineBookManagementSystem.Presentation.ViewModels.Users.UserManagementViewModel
            {
                Users = viewModel.Users.Select(u => new OnlineBookManagementSystem.Presentation.ViewModels.Users.UserManagementItem
                {
                    Id = u.Id,
                    Name = u.Name,
                    UserName = u.UserName,
                    Email = u.Email,
                    Role = u.Role,
                    RequestedRole = u.RequestedRole,
                    IsDeleted = u.IsDeleted,
                    IsPendingApproval = u.IsPendingApproval,
                    EmailConfirmed = u.EmailConfirmed,
                    LockoutEnd = u.LockoutEnd,
                    LastLoginDate = u.LastLoginDate,
                    CreatedDate = u.CreatedDate
                }).ToList(),

                Filters = new OnlineBookManagementSystem.Presentation.ViewModels.Users.UserManagementFilters
                {
                    SearchTerm = search,
                    RoleFilter = role,
                    StatusFilter = status
                },

                Capabilities = new OnlineBookManagementSystem.Presentation.ViewModels.Users.UserManagementCapabilities
                {
                    CanView = true,
                    CanCreate = true, // SuperAdmin can create users
                    CanEdit = true,
                    CanDelete = true, // SuperAdmin can delete users
                    CanChangeRoles = true, // SuperAdmin can change roles
                    CanLockUnlock = true, // SuperAdmin can lock/unlock
                    CanViewSensitiveData = true, // SuperAdmin has full access
                    CanExport = true, // SuperAdmin can export
                    CanViewAllUsers = true, // SuperAdmin sees all users
                    CanManageSuperAdmins = true // SuperAdmin can manage other SuperAdmins
                },

                CurrentPage = viewModel.CurrentPage,
                TotalPages = viewModel.TotalPages,
                TotalUsers = viewModel.TotalUsers,
                PageSize = 20,
                ActiveUsers = viewModel.Users.Count(u => !u.IsDeleted && u.LockoutEnd <= DateTimeOffset.UtcNow),
                InactiveUsers = viewModel.Users.Count(u => u.IsDeleted || u.LockoutEnd > DateTimeOffset.UtcNow),
                PendingUsers = viewModel.Users.Count(u => u.IsPendingApproval)
            };

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;

            await _activityLogger.LogAsync("ManageUsers", "User management page accessed", userId);
            return View("~/Presentation/Views/Users/UserManagement.cshtml", unifiedViewModel);
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

            // Default to today's logs if no date filters are provided
            var viewModel = await _activityLogger.GetActivityLogsAsync(page, 50, search, dateFrom, dateTo);

            ViewBag.Search = search;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.ShowingToday = !dateFrom.HasValue && !dateTo.HasValue;

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
                var user = EscapeCsv(log.User?.Email ?? "System");
                var desc = EscapeCsv(log.Description ?? "");
                var actionType = EscapeCsv(log.ActionType ?? "");
                var ip = EscapeCsv(log.IpAddress ?? "");
                var agent = EscapeCsv(log.UserAgent ?? "");
                csv.AppendLine($"{log.Timestamp},{actionType},{desc},{user},{ip},{agent}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"ActivityLogs_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        public async Task<IActionResult> ExportUsers(string? search = null, string? role = null, string? status = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var viewModel = await _usersService.GetManageUsersDataAsync(1, 100000, search, role, status); // Get all matching

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Name,Username,Email,Role,Status,Email Confirmed,Created Date,Last Login");

                foreach (var user in viewModel.Users)
                {
                    var name = EscapeCsv(user.Name ?? "");
                    var username = EscapeCsv(user.UserName ?? "");
                    var email = EscapeCsv(user.Email ?? "");
                    var userRole = EscapeCsv(user.Role ?? "");
                    var userStatus = user.IsDeleted ? "Deleted" :
                                   user.LockoutEnd > DateTimeOffset.UtcNow ? "Locked" :
                                   user.IsPendingApproval ? "Pending" : "Active";
                    var emailConfirmed = user.EmailConfirmed ? "Yes" : "No";
                    var createdDate = user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var lastLogin = user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";

                    csv.AppendLine($"{user.Id},{name},{username},{email},{userRole},{userStatus},{emailConfirmed},{createdDate},{lastLogin}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"Users_{DateTime.Now:yyyyMMddHHmmss}.csv";

                await _activityLogger.LogAsync("ExportUsers", "User data exported", userId);
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export users");
                return Json(new { success = false, message = "Failed to export user data" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var serviceRequest = new Presentation.ViewModels.SuperAdmin.CreateUserRequest
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    Role = request.Role
                };

                var result = await _usersService.CreateUserAsync(serviceRequest);
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        // Unified action endpoint for quick operations
        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteQuickAction([FromBody] QuickActionRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = request.Action.ToLower() switch
                {
                    "clearcache" => await ExecuteClearCacheAsync(),
                    "backupdatabase" => await ConvertToServiceResult(await _systemSettingsService.BackupDatabaseAsync()),
                    "testemail" => await ConvertToServiceResult(await _systemSettingsService.TestEmailConfigurationAsync()),
                    "clearoldlogs" => await ExecuteClearOldLogsAsync(request.Days ?? 30),
                    "approveuser" => await ConvertToServiceResult(await _usersService.ApproveUserAsync(request.UserId ?? 0, request.Role ?? "User")),
                    "rejectuser" => await ConvertToServiceResult(await _usersService.RejectUserAsync(request.UserId ?? 0)),
                    _ => new ServiceResult { Success = false, Message = "Invalid action" }
                };

                if (result.Success)
                {
                    await _activityLogger.LogAsync("QuickAction", $"Executed {request.Action}", userId);
                }

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while executing the action" });
            }
        }

        // System overview endpoint for dashboard widgets
        [HttpGet]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> GetSystemOverview()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var dashboardData = await _usersService.GetSuperAdminDashboardDataAsync();
                var systemSettings = await _systemSettingsService.GetSystemSettingsAsync();
                var pendingUsers = await _usersService.GetPendingUsersAsync();

                var overview = new
                {
                    stats = new
                    {
                        totalUsers = dashboardData.TotalUsers,
                        newUsersToday = dashboardData.NewUsersToday,
                        totalBooks = dashboardData.TotalBooks,
                        totalOrders = dashboardData.TotalOrders,
                        totalRevenue = dashboardData.TotalRevenue,
                        activeSession = dashboardData.ActiveSessions
                    },
                    systemHealth = new
                    {
                        database = "Connected",
                        cache = "Active",
                        email = "Configured",
                        storage = $"{dashboardData.StorageUsagePercent}%"
                    },
                    pendingItems = new
                    {
                        users = pendingUsers.Count,
                        maintenanceMode = systemSettings.MaintenanceMode
                    }
                };

                return Json(new { success = true, data = overview });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to get system overview" });
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

        // Role Management Methods - Missing functionality added
        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteUser(int userId, string newRole)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // Validate role
                var validRoles = new[] { "User", "Admin", "SuperAdmin" };
                if (!validRoles.Contains(newRole))
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }

                // Prevent non-SuperAdmin from creating SuperAdmin
                var currentUser = await _userManager.FindByIdAsync(adminId.ToString());
                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                if (newRole == "SuperAdmin" && !currentUserRoles.Contains("SuperAdmin"))
                {
                    return Json(new { success = false, message = "Only SuperAdmin can promote to SuperAdmin role" });
                }

                var result = await _usersService.UpdateUserRoleAsync(userId, newRole);
                if (result)
                {
                    await _activityLogger.LogAsync("PromoteUser", $"User {userId} promoted to {newRole}", adminId);
                    return Json(new { success = true, message = $"User successfully promoted to {newRole}" });
                }
                return Json(new { success = false, message = "Failed to promote user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to promote user {UserId} to {Role}", userId, newRole);
                return Json(new { success = false, message = "An error occurred while promoting the user" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteUser(int userId, string newRole)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // Validate role
                var validRoles = new[] { "User", "Admin" };
                if (!validRoles.Contains(newRole))
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }

                // Prevent demoting self
                if (userId == adminId)
                {
                    return Json(new { success = false, message = "Cannot demote yourself" });
                }

                var result = await _usersService.UpdateUserRoleAsync(userId, newRole);
                if (result)
                {
                    await _activityLogger.LogAsync("DemoteUser", $"User {userId} demoted to {newRole}", adminId);
                    return Json(new { success = true, message = $"User successfully demoted to {newRole}" });
                }
                return Json(new { success = false, message = "Failed to demote user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to demote user {UserId} to {Role}", userId, newRole);
                return Json(new { success = false, message = "An error occurred while demoting the user" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(int userId, string newRole)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // Validate role
                var validRoles = new[] { "User", "Admin", "SuperAdmin", "Guest", "Public" };
                if (!validRoles.Contains(newRole))
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }

                // Get current user and target user
                var currentUser = await _userManager.FindByIdAsync(adminId.ToString());
                var targetUser = await _userManager.FindByIdAsync(userId.ToString());

                if (targetUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

                // Security checks
                if (userId == adminId)
                {
                    return Json(new { success = false, message = "Cannot change your own role" });
                }

                if (newRole == "SuperAdmin" && !currentUserRoles.Contains("SuperAdmin"))
                {
                    return Json(new { success = false, message = "Only SuperAdmin can assign SuperAdmin role" });
                }

                if (targetUserRoles.Contains("SuperAdmin") && !currentUserRoles.Contains("SuperAdmin"))
                {
                    return Json(new { success = false, message = "Only SuperAdmin can modify SuperAdmin users" });
                }

                var result = await _usersService.UpdateUserRoleAsync(userId, newRole);
                if (result)
                {
                    await _activityLogger.LogAsync("ChangeUserRole",
                        $"User {targetUser.Email} role changed to {newRole}", adminId);
                    return Json(new { success = true, message = $"User role successfully changed to {newRole}" });
                }
                return Json(new { success = false, message = "Failed to change user role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change user {UserId} role to {Role}", userId, newRole);
                return Json(new { success = false, message = "An error occurred while changing the user role" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                if (userId == adminId)
                {
                    return Json(new { success = false, message = "Cannot toggle your own status" });
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var isCurrentlyActive = !(bool)user.IsDeleted && user.LockoutEnd == null;
                var result = await _usersService.ToggleUserStatusAsync(userId, !isCurrentlyActive);

                if (result)
                {
                    var action = isCurrentlyActive ? "deactivated" : "activated";
                    await _activityLogger.LogAsync("ToggleUserStatus",
                        $"User {user.Email} {action}", adminId);
                    return Json(new { success = true, message = $"User successfully {action}" });
                }
                return Json(new { success = false, message = "Failed to toggle user status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle status for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while toggling user status" });
            }
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // Prevent formula injection (CSV Injection)
            if (field.StartsWith("=") || field.StartsWith("+") || field.StartsWith("-") || field.StartsWith("@"))
            {
                field = "'" + field;
            }

            // Escape quotes and wrap in quotes if necessary
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        // Helper methods for unified actions
        private async Task<ServiceResult> ExecuteClearCacheAsync()
        {
            await _systemSettingsService.ClearCacheAsync();
            return new ServiceResult { Success = true, Message = "Cache cleared successfully" };
        }

        private async Task<ServiceResult> ExecuteClearOldLogsAsync(int days)
        {
            var result = await _activityLogger.ClearOldLogsAsync(days);
            return new ServiceResult { Success = true, Message = $"Cleared {result} old log entries" };
        }

        private Task<ServiceResult> ConvertToServiceResult((bool Success, string Message) tuple)
        {
            return Task.FromResult(new ServiceResult { Success = tuple.Success, Message = tuple.Message });
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

    // Enhanced request models for unified actions
    public class QuickActionRequest
    {
        public string Action { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Role { get; set; }
        public int? Days { get; set; }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    // Enhanced view models for consolidated dashboard
    public class EnhancedSuperAdminDashboardViewModel : SuperAdminDashboardViewModel
    {
        public SystemInfoSummary SystemInfo { get; set; } = new();
        public int PendingUsersCount { get; set; }
        public List<UserWithRoleViewModel> PendingUsers { get; set; } = new();
        public List<QuickAction> QuickActions { get; set; } = new();


    }

    public class SystemInfoSummary
    {
        public string AppVersion { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; }
        public string ServerUptime { get; set; } = string.Empty;
    }

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }


}
