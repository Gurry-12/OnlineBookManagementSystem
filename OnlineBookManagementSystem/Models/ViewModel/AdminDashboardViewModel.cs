using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockBooks { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Recent Activity
        public List<ActivityLog> RecentActivity { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public List<Book> RecentBooks { get; set; } = new();

        // Chart Data
        public List<MonthlyBookUploadViewModel> MonthlyBookUploads { get; set; } = new();
        public List<CategoryBookCountViewModel> BooksByCategory { get; set; } = new();
        public List<AuthorBookCountViewModel> BooksByAuthor { get; set; } = new();
        public FavoriteStatsViewModel FavoriteStats { get; set; } = new();
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();
        public List<OrderStatusViewModel> OrdersByStatus { get; set; } = new();
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderStatusViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}