namespace OnlineBookManagementSystem.Models.ViewModel;

public class SuperAdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int TotalBooks { get; set; }
    public int BooksAddedThisMonth { get; set; }
    public int TotalOrders { get; set; }
    public int OrdersToday { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public int StorageUsagePercent { get; set; }
    public int ActiveSessions { get; set; }
    public List<ActivityLog> RecentActivities { get; set; } = new();
    public List<ActivityLog> RecentActivity { get; set; } = new(); // For backward compatibility
}