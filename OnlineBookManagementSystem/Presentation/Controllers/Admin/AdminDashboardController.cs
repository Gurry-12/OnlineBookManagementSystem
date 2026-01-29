using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Activity;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    /// <summary>
    /// Handles admin dashboard functionality following SRP.
    /// Responsible only for dashboard data aggregation and display.
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    public class AdminDashboardController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookAnalyticsService _bookAnalyticsService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly IUsersService _userService;
        private readonly ICategoryInterface _categoryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            IBookQueryService bookQueryService,
            IBookAnalyticsService bookAnalyticsService,
            IOrderQueryService orderQueryService,
            IUsersService userService,
            ICategoryInterface categoryService,
            IActivityLogger activityLogger,
            ILogger<AdminDashboardController> logger)
        {
            _bookQueryService = bookQueryService;
            _bookAnalyticsService = bookAnalyticsService;
            _orderQueryService = orderQueryService;
            _userService = userService;
            _categoryService = categoryService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await GetAdminDashboardDataAsync(userId);
                await _activityLogger.LogAsync("Dashboard", "Admin dashboard accessed", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load dashboard data.";
                return View(new AdminDashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var data = chartType switch
                {
                    "monthly" => (object)await _bookAnalyticsService.GetMonthlyBookUploadsAsync(),
                    "category" => (object)await _bookAnalyticsService.GetBooksByCategoryAsync(),
                    "author" => (object)await _bookAnalyticsService.GetBooksByAuthorAsync(),
                    "favorites" => (object)await _bookAnalyticsService.GetFavoriteStatsAsync(),
                    "revenue" => (object)await _orderQueryService.GetMonthlyRevenueAsync(),
                    "orderStatus" => (object)await _orderQueryService.GetOrderStatusDistributionAsync(),
                    _ => (object?)null
                };

                if (data == null)
                {
                    return Json(new { success = false, message = "Invalid chart type" });
                }

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chart data for type {ChartType}", chartType);
                return Json(new { success = false, message = "Failed to load chart data", error = ex.Message });
            }
        }

        public async Task<IActionResult> ActivityLogs(int page = 1, string? search = null, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                // Default to today's logs if no filters are applied
                List<ActivityLogViewModel> viewModel;

                if (string.IsNullOrEmpty(search) && string.IsNullOrEmpty(action) && !dateFrom.HasValue && !dateTo.HasValue)
                {
                    // Show today's logs by default
                    viewModel = await _activityLogger.GetTodayLogsAsync();
                    ViewBag.ShowingToday = true;
                }
                else
                {
                    // Show filtered logs
                    viewModel = await _activityLogger.GetFilteredLogsAsync(dateFrom, dateTo, search, action);
                    ViewBag.ShowingToday = false;
                }

                ViewBag.Search = search;
                ViewBag.Action = action;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity logs");
                TempData["ErrorMessage"] = "Failed to load activity logs.";
                return View(new List<ActivityLogViewModel>());
            }
        }

        private async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(int userId)
        {
            var totalBooks = await _bookQueryService.GetTotalBooksCountAsync();
            var totalOrders = await _orderQueryService.GetTotalOrdersCountAsync();
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var totalCategories = await _categoryService.GetTotalCategoriesCountAsync();

            // Get today's activities only for dashboard
            var todayActivities = await _activityLogger.GetTodayLogsAsync();
            var recentActivities = todayActivities.Take(8).Select(log => new ActivityLog
            {
                Id = 0, // Not needed for display
                Action = log.Action,
                Message = log.Description,
                Timestamp = log.Timestamp,
                UserId = null, // Will be handled in view
                User = null // Will show as System or get from UserName
            }).ToList();

            var monthlyStats = await _bookAnalyticsService.GetMonthlyStatsAsync();

            return new AdminDashboardViewModel
            {
                TotalBooks = totalBooks,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                RecentActivity = recentActivities,
                MonthlyBookUploads = monthlyStats.MonthlyUploads,
                BooksByCategory = monthlyStats.CategoryDistribution,
                BooksByAuthor = monthlyStats.AuthorDistribution,
                FavoriteStats = monthlyStats.FavoriteStats
            };
        }
    }
}