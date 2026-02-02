using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

namespace OnlineBookManagementSystem.Presentation.Controllers;

[Authorize(Policy = "AdminOrHigher")]
public class AnalyticsController : BaseController
{
    private readonly IBookAnalyticsService _bookAnalyticsService;
    private readonly IOrderQueryService _orderQueryService;
    private readonly IUserQueryService _userQueryService;
    private readonly IReviewService _reviewService;

    public AnalyticsController(
        IBookAnalyticsService bookAnalyticsService,
        IOrderQueryService orderQueryService,
        IUserQueryService userQueryService,
        IReviewService reviewService)
    {
        _bookAnalyticsService = bookAnalyticsService;
        _orderQueryService = orderQueryService;
        _userQueryService = userQueryService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> OrdersAnalytics(DateTime? startDate = null, DateTime? endDate = null, string period = "Last 30 Days")
    {
        try
        {
            var filters = CreateFilters(startDate, endDate, period);
            var capabilities = GetAnalyticsCapabilities();

            if (!capabilities.CanView)
            {
                return View(new AnalyticsViewModel<OrdersAnalyticsData>
                {
                    Capabilities = capabilities,
                    Filters = filters,
                    Data = new OrdersAnalyticsData()
                });
            }

            // Get orders analytics data
            var monthlyRevenue = await _orderQueryService.GetMonthlyRevenueAsync();
            var orderStatusDistribution = await _orderQueryService.GetOrderStatusDistributionAsync();
            var totalRevenue = await _orderQueryService.GetTotalRevenueAsync();
            var totalOrders = await _orderQueryService.GetTotalOrdersCountAsync();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            var data = new OrdersAnalyticsData
            {
                MonthlyRevenue = monthlyRevenue.ToList(),
                OrderStatusDistribution = orderStatusDistribution.Select(o => new OrderStatusDistribution
                {
                    Status = o.Status,
                    Count = o.Count,
                    Revenue = o.TotalAmount,
                    Percentage = (double)o.Percentage
                }).ToList(),
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                PendingOrders = 0, // await _orderQueryService.GetPendingOrdersCountAsync(),
                CompletedOrders = 0, // await _orderQueryService.GetCompletedOrdersCountAsync(),
                CancelledOrders = 0, // await _orderQueryService.GetCancelledOrdersCountAsync(),
                RevenueGrowthRate = 0, // capabilities.CanViewSensitiveMetrics ? await _orderQueryService.GetRevenueGrowthRateAsync() : 0,
                OrderGrowthRate = 0, // capabilities.CanViewSensitiveMetrics ? await _orderQueryService.GetOrderGrowthRateAsync() : 0,
                DailyOrderTrends = await GetDailyOrderTrends(filters.StartDate, filters.EndDate)
            };

            var viewModel = new AnalyticsViewModel<OrdersAnalyticsData>
            {
                Data = data,
                Filters = filters,
                Capabilities = capabilities
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading orders analytics.";
            return RedirectToAction("Dashboard", GetDashboardController());
        }
    }

    [HttpGet]
    public async Task<IActionResult> BooksAnalytics(DateTime? startDate = null, DateTime? endDate = null, string period = "Last 30 Days")
    {
        try
        {
            var filters = CreateFilters(startDate, endDate, period);
            var capabilities = GetAnalyticsCapabilities();

            if (!capabilities.CanView)
            {
                return View(new AnalyticsViewModel<BooksAnalyticsData>
                {
                    Capabilities = capabilities,
                    Filters = filters,
                    Data = new BooksAnalyticsData()
                });
            }

            var data = new BooksAnalyticsData
            {
                MonthlyUploads = (await _bookAnalyticsService.GetMonthlyBookUploadsAsync()).ToList(),
                CategoryDistribution = (await _bookAnalyticsService.GetBooksByCategoryAsync()).ToList(),
                AuthorDistribution = (await _bookAnalyticsService.GetBooksByAuthorAsync()).ToList(),
                FavoriteStats = await _bookAnalyticsService.GetFavoriteStatsAsync(),
                TotalBooks = 0, // await _bookAnalyticsService.GetTotalBooksCountAsync(),
                ActiveBooks = 0, // await _bookAnalyticsService.GetActiveBooksCountAsync(),
                BooksThisMonth = 0, // await _bookAnalyticsService.GetBooksThisMonthCountAsync(),
                TotalCategories = 0, // await _bookAnalyticsService.GetTotalCategoriesCountAsync(),
                TotalAuthors = 0, // await _bookAnalyticsService.GetTotalAuthorsCountAsync(),
                MostPopularBooks = await GetMostPopularBooks(),
                MostFavoritedBooks = await GetMostFavoritedBooks()
            };

            var viewModel = new AnalyticsViewModel<BooksAnalyticsData>
            {
                Data = data,
                Filters = filters,
                Capabilities = capabilities
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading books analytics.";
            return RedirectToAction("Dashboard", GetDashboardController());
        }
    }

    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> UsersAnalytics(DateTime? startDate = null, DateTime? endDate = null, string period = "Last 30 Days")
    {
        try
        {
            var filters = CreateFilters(startDate, endDate, period);
            var capabilities = GetAnalyticsCapabilities();

            if (!capabilities.CanView || !capabilities.CanViewSensitiveMetrics)
            {
                return View(new AnalyticsViewModel<UsersAnalyticsData>
                {
                    Capabilities = capabilities,
                    Filters = filters,
                    Data = new UsersAnalyticsData()
                });
            }

            var data = new UsersAnalyticsData
            {
                TotalUsers = await _userQueryService.GetTotalUsersCountAsync(),
                ActiveUsers = await _userQueryService.GetActiveUsersCountAsync(),
                NewUsersThisMonth = 0, // await _userQueryService.GetNewUsersThisMonthAsync(),
                InactiveUsers = 0, // await _userQueryService.GetInactiveUsersCountAsync(),
                UserGrowthRate = 0, // await _userQueryService.GetUserGrowthRateAsync(),
                ActivityRate = 0, // await _userQueryService.GetActivityRateAsync(),
                RetentionRate = 0, // await _userQueryService.GetRetentionRateAsync(),
                UserGrowthData = await GetUserGrowthData(filters.StartDate, filters.EndDate),
                ActivityTrends = await GetUserActivityTrends(filters.StartDate, filters.EndDate),
                RoleDistribution = await GetRoleDistribution()
            };

            var viewModel = new AnalyticsViewModel<UsersAnalyticsData>
            {
                Data = data,
                Filters = filters,
                Capabilities = capabilities
            };

            ViewData["Layout"] = "_LayoutSuperAdmin";
            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading users analytics.";
            return RedirectToAction("Dashboard", "SuperAdmin");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ReviewsAnalytics(DateTime? startDate = null, DateTime? endDate = null, string period = "Last 30 Days")
    {
        try
        {
            var filters = CreateFilters(startDate, endDate, period);
            var capabilities = GetAnalyticsCapabilities();

            if (!capabilities.CanView)
            {
                return View(new AnalyticsViewModel<ReviewsAnalyticsData>
                {
                    Capabilities = capabilities,
                    Filters = filters,
                    Data = new ReviewsAnalyticsData()
                });
            }

            var reviewAnalytics = await _reviewService.GetReviewAnalyticsAsync();

            var data = new ReviewsAnalyticsData
            {
                TotalReviews = reviewAnalytics.TotalReviews,
                PendingReviews = reviewAnalytics.PendingReviews,
                ApprovedReviews = reviewAnalytics.ApprovedReviews,
                RejectedReviews = reviewAnalytics.RejectedReviews,
                FlaggedReviews = reviewAnalytics.FlaggedReviews,
                AverageRating = reviewAnalytics.AverageRating,
                ReviewsThisMonth = 0, // await _reviewService.GetReviewsThisMonthAsync(),
                ApprovalRate = reviewAnalytics.TotalReviews > 0 ? (double)reviewAnalytics.ApprovedReviews / reviewAnalytics.TotalReviews : 0,
                RejectionRate = reviewAnalytics.TotalReviews > 0 ? (double)reviewAnalytics.RejectedReviews / reviewAnalytics.TotalReviews : 0,
                ReviewTrends = reviewAnalytics.ReviewTrends.Select(t => new ReviewTrendViewModel
                {
                    Date = t.Date,
                    ReviewCount = t.ReviewCount,
                    AverageRating = t.AverageRating,
                    ApprovedCount = 0, // Would need to extend service
                    RejectedCount = 0  // Would need to extend service
                }).ToList(),
                RatingDistribution = reviewAnalytics.RatingDistribution.Select(r => new RatingDistributionViewModel
                {
                    Rating = r.Key,
                    Count = r.Value,
                    Percentage = reviewAnalytics.TotalReviews > 0 ? (double)r.Value / reviewAnalytics.TotalReviews * 100 : 0
                }).ToList(),
                ModerationStats = await GetReviewModerationStats(filters.StartDate, filters.EndDate)
            };

            var viewModel = new AnalyticsViewModel<ReviewsAnalyticsData>
            {
                Data = data,
                Filters = filters,
                Capabilities = capabilities
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading reviews analytics.";
            return RedirectToAction("Dashboard", GetDashboardController());
        }
    }

    // Helper methods
    private AnalyticsFilters CreateFilters(DateTime? startDate, DateTime? endDate, string period)
    {
        var filters = new AnalyticsFilters { Period = period };

        if (startDate.HasValue && endDate.HasValue)
        {
            filters.StartDate = startDate.Value;
            filters.EndDate = endDate.Value;
            filters.Period = "Custom";
        }
        else
        {
            var (start, end) = GetDateRangeFromPeriod(period);
            filters.StartDate = start;
            filters.EndDate = end;
        }

        return filters;
    }

    private (DateTime start, DateTime end) GetDateRangeFromPeriod(string period)
    {
        var end = DateTime.Now;
        var start = period switch
        {
            "Last 7 Days" => end.AddDays(-7),
            "Last 30 Days" => end.AddDays(-30),
            "Last 90 Days" => end.AddDays(-90),
            "This Year" => new DateTime(end.Year, 1, 1),
            _ => end.AddDays(-30)
        };
        return (start, end);
    }

    private AnalyticsCapabilities GetAnalyticsCapabilities()
    {
        return new AnalyticsCapabilities
        {
            CanView = CanAccessAdmin(),
            CanExport = CanAccessSuperAdmin(), // Only SuperAdmin can export
            CanViewSensitiveMetrics = CanAccessSuperAdmin() // Only SuperAdmin sees sensitive metrics
        };
    }

    private string GetDashboardController()
    {
        return CanAccessSuperAdmin() ? "SuperAdmin" : "Admin";
    }

    // Placeholder methods - would need to implement these in services
    private async Task<List<DailyOrderTrend>> GetDailyOrderTrends(DateTime startDate, DateTime endDate)
    {
        // Implementation would go here
        return new List<DailyOrderTrend>();
    }

    private async Task<List<PopularBookViewModel>> GetMostPopularBooks()
    {
        // Implementation would go here
        return new List<PopularBookViewModel>();
    }

    private async Task<List<PopularBookViewModel>> GetMostFavoritedBooks()
    {
        // Implementation would go here
        return new List<PopularBookViewModel>();
    }

    private async Task<List<UserGrowthDataViewModel>> GetUserGrowthData(DateTime startDate, DateTime endDate)
    {
        // Implementation would go here
        return new List<UserGrowthDataViewModel>();
    }

    private async Task<List<UserActivityTrend>> GetUserActivityTrends(DateTime startDate, DateTime endDate)
    {
        // Implementation would go here
        return new List<UserActivityTrend>();
    }

    private async Task<List<RoleDistribution>> GetRoleDistribution()
    {
        // Implementation would go here
        return new List<RoleDistribution>();
    }

    private async Task<List<ReviewModerationStats>> GetReviewModerationStats(DateTime startDate, DateTime endDate)
    {
        // Implementation would go here
        return new List<ReviewModerationStats>();
    }
}
