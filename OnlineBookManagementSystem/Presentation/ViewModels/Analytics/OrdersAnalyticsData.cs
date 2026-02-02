using OnlineBookManagementSystem.Presentation.ViewModels.Admin;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

public class OrdersAnalyticsData
{
    public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();
    public List<OrderStatusDistribution> OrderStatusDistribution { get; set; } = new();
    public List<DailyOrderTrend> DailyOrderTrends { get; set; } = new();

    // Summary metrics
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }

    // Growth metrics (sensitive)
    public double RevenueGrowthRate { get; set; }
    public double OrderGrowthRate { get; set; }
}

public class OrderStatusDistribution
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
}

public class DailyOrderTrend
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}