using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;
namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int BooksAddedThisMonth { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int OrdersThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog> RecentActivity { get; set; } = new();
    public List<MonthlyBookUploadViewModel> MonthlyBookUploads { get; set; } = new();
    public List<CategoryBookCountViewModel> BooksByCategory { get; set; } = new();
    public List<AuthorBookCountViewModel> BooksByAuthor { get; set; } = new();
    public FavoriteStatsViewModel FavoriteStats { get; set; } = new();
    public int TotalCategories { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockBooks { get; set; }
    public int OutOfStockBooks { get; set; }
    public List<MonthlyStatsViewModel> MonthlyStats { get; set; } = new();

    // Additional properties for carousel
    public int RecentBooksCount { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int ActiveUsersToday { get; set; }
}

public class MonthlyStatsViewModel
{
    public string Month { get; set; } = string.Empty;
    public int BooksAdded { get; set; }
    public int OrdersPlaced { get; set; }
    public decimal Revenue { get; set; }
}
