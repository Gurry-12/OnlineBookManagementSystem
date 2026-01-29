using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

namespace OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

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
    
    // Additional properties for analytics
    public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();
    public List<AdminOrderItemViewModel> RecentOrders { get; set; } = new();
    public List<OrderStatusViewModel> OrderStatusDistribution { get; set; } = new();
    public AdminMonthlyStatsViewModel MonthlyStats { get; set; } = new();
    
    // Additional properties
    public int PendingUsersCount { get; set; }
    public List<Core.Domain.Entities.User> PendingUsers { get; set; } = new();
    public SystemInfoViewModel SystemInfo { get; set; } = new();
    public List<QuickActionViewModel> QuickActions { get; set; } = new();
}

public class SystemInfoViewModel
{
    public string ServerTime { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public int CpuUsage { get; set; }
    public int MemoryUsage { get; set; }
    public bool MaintenanceMode { get; set; }
    public TimeSpan ServerUptime { get; set; }
}

public class QuickActionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}