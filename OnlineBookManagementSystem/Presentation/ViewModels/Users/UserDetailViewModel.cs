using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Users;

public class UserDetailViewModel
{
    public UserDetailsViewModel User { get; set; } = null!;
    public UserManagementCapabilities Capabilities { get; set; } = new();
    public bool IsEditMode { get; set; }

    // User activity summary
    public int TotalOrders { get; set; }
    public int TotalReviews { get; set; }
    public int FavoriteBooks { get; set; }
    public DateTime? LastActivity { get; set; }

    // Recent activity
    public List<ActivityLog> RecentActivity { get; set; } = new();

    // Role change history (if SuperAdmin)
    public List<RoleChangeHistory> RoleHistory { get; set; } = new();
}

public class UserDetailCapabilities
{
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanChangeRoles { get; set; }
    public bool CanLockUnlock { get; set; }
    public bool CanViewSensitiveData { get; set; }
    public bool CanViewAllUsers { get; set; }
    public bool CanManageSuperAdmins { get; set; }
}

public class RoleChangeHistory
{
    public DateTime ChangedAt { get; set; }
    public string FromRole { get; set; } = string.Empty;
    public string ToRole { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}