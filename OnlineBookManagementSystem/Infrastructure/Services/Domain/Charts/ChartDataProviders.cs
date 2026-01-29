

using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;



namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Charts;

public class MonthlyChartDataProvider : IChartDataProvider
{
    private readonly IBookAnalyticsService _bookAnalyticsService;

    public string ChartType => "monthly";

    public MonthlyChartDataProvider(IBookAnalyticsService bookAnalyticsService)
    {
        _bookAnalyticsService = bookAnalyticsService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _bookAnalyticsService.GetMonthlyBookUploadsAsync();
    }
}

public class CategoryChartDataProvider : IChartDataProvider
{
    private readonly IBookAnalyticsService _bookAnalyticsService;

    public string ChartType => "category";

    public CategoryChartDataProvider(IBookAnalyticsService bookAnalyticsService)
    {
        _bookAnalyticsService = bookAnalyticsService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _bookAnalyticsService.GetBooksByCategoryAsync();
    }
}

public class AuthorChartDataProvider : IChartDataProvider
{
    private readonly IBookAnalyticsService _bookAnalyticsService;

    public string ChartType => "author";

    public AuthorChartDataProvider(IBookAnalyticsService bookAnalyticsService)
    {
        _bookAnalyticsService = bookAnalyticsService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _bookAnalyticsService.GetBooksByAuthorAsync();
    }
}

public class FavoritesChartDataProvider : IChartDataProvider
{
    private readonly IBookAnalyticsService _bookAnalyticsService;

    public string ChartType => "favorites";

    public FavoritesChartDataProvider(IBookAnalyticsService bookAnalyticsService)
    {
        _bookAnalyticsService = bookAnalyticsService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _bookAnalyticsService.GetFavoriteStatsAsync();
    }
}

public class RevenueChartDataProvider : IChartDataProvider
{
    private readonly IOrderQueryService _orderQueryService;

    public string ChartType => "revenue";

    public RevenueChartDataProvider(IOrderQueryService orderQueryService)
    {
        _orderQueryService = orderQueryService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _orderQueryService.GetMonthlyRevenueAsync();
    }
}

public class OrderStatusChartDataProvider : IChartDataProvider
{
    private readonly IOrderQueryService _orderQueryService;

    public string ChartType => "orderStatus";

    public OrderStatusChartDataProvider(IOrderQueryService orderQueryService)
    {
        _orderQueryService = orderQueryService;
    }

    public async Task<object> GetDataAsync()
    {
        return await _orderQueryService.GetOrderStatusDistributionAsync();
    }
}
