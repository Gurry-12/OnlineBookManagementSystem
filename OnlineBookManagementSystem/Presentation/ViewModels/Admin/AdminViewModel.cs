using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Activity;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin;

public class AdminViewModel
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public Core.Domain.Entities.User User { get; set; }

    public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();
    public List<ActivityLog> RecentActivity { get; internal set; }
}