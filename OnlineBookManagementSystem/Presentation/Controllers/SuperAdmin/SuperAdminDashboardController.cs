using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;
using OnlineBookManagementSystem.Core.Application.Mappings;
using static OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin.SystemHealthViewModel;

namespace OnlineBookManagementSystem.Presentation.Controllers.SuperAdmin
{
    /// <summary>
    /// Focused controller for SuperAdmin dashboard operations only.
    /// Follows Single Responsibility Principle - only handles dashboard-related actions.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [Route("SuperAdmin/Dashboard")]
    public class SuperAdminDashboardController : BaseController
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly IUserQueryService _userQueryService;
        private readonly IBookAnalyticsService _analyticsService;

        public SuperAdminDashboardController(
            IOrderQueryService orderQueryService,
            IUserQueryService userQueryService,
            IBookAnalyticsService analyticsService)
        {
            _orderQueryService = orderQueryService;
            _userQueryService = userQueryService;
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new SuperAdminDashboardViewModel
                {
                    TotalUsers = await _userQueryService.GetTotalUsersCountAsync(),
                    TotalOrders = await _orderQueryService.GetTotalOrdersCountAsync(),
                    TotalRevenue = await _orderQueryService.GetTotalRevenueAsync(),
                    MonthlyRevenue = await _orderQueryService.GetMonthlyRevenueAsync(DateTime.Now.Year, DateTime.Now.Month),
                    RecentOrders = (await _orderQueryService.GetRecentOrdersAsync(10))
                        .Select(o => o.ToAdminOrderItem())
                        .ToList(),
                    OrderStatusDistribution = await _orderQueryService.GetOrderStatusDistributionAsync(),
                    MonthlyStats = await _analyticsService.GetMonthlyStatsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading SuperAdmin dashboard");
                return View("Error");
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var viewModel = new SuperAdminAnalyticsViewModel
                {
                    UserGrowthData = await _analyticsService.GetUserGrowthDataAsync(),
                    RevenueGrowthData = await _analyticsService.GetRevenueGrowthDataAsync(),
                    BookPopularityData = await _analyticsService.GetBookPopularityDataAsync(),
                    CategoryDistribution = await _analyticsService.GetCategoryDistributionAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading SuperAdmin analytics");
                return View("Error");
            }
        }

        [HttpGet("system-health")]
        public async Task<IActionResult> SystemHealth()
        {
            try
            {
                var viewModel = new SystemHealthViewModel
                {
                    DatabaseStatus = new DatabaseHealthViewModel 
                    { 
                        Status = await CheckDatabaseHealthAsync() == "Healthy" ? HealthStatus.Healthy : HealthStatus.Critical,
                        IsConnected = await CheckDatabaseHealthAsync() == "Healthy"
                    },
                    CacheStatus = new CacheHealthViewModel 
                    { 
                        Status = await CheckCacheHealthAsync() == "Healthy" ? HealthStatus.Healthy : HealthStatus.Critical,
                        IsConnected = await CheckCacheHealthAsync() == "Healthy"
                    },
                    EmailServiceStatus = await CheckEmailServiceHealthAsync(),
                    SystemUptime = GetSystemUptime(),
                    ActiveUsers = await _userQueryService.GetActiveUsersCountAsync(),
                    Performance = new PerformanceMetricsViewModel
                    {
                        MemoryUsage = GetMemoryUsage() / (1024.0 * 1024.0) // Convert to MB
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading system health");
                return View("Error");
            }
        }

        private async Task<string> CheckDatabaseHealthAsync()
        {
            try
            {
                await _userQueryService.GetTotalUsersCountAsync();
                return "Healthy";
            }
            catch
            {
                return "Unhealthy";
            }
        }

        private async Task<string> CheckCacheHealthAsync()
        {
            // Implementation for cache health check
            return "Healthy";
        }

        private async Task<string> CheckEmailServiceHealthAsync()
        {
            // Implementation for email service health check
            return "Healthy";
        }

        private TimeSpan GetSystemUptime()
        {
            return DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        }

        private long GetMemoryUsage()
        {
            return GC.GetTotalMemory(false);
        }
    }
}