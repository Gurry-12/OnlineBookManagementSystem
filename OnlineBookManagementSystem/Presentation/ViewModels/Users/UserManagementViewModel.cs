namespace OnlineBookManagementSystem.Presentation.ViewModels.Users;

public class UserManagementViewModel
{
    public List<UserManagementItem> Users { get; set; } = new();
    public UserManagementFilters Filters { get; set; } = new();
    public UserManagementCapabilities Capabilities { get; set; } = new();

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }
    public int PageSize { get; set; } = 20;

    // Summary stats
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int PendingUsers { get; set; }
}

public class UserManagementItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? RequestedRole { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPendingApproval { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Computed properties
    public string Status => IsDeleted ? "Deleted" :
                           LockoutEnd > DateTimeOffset.UtcNow ? "Locked" :
                           IsPendingApproval ? "Pending" :
                           "Active";

    public string StatusBadgeClass => Status switch
    {
        "Active" => "bg-success",
        "Pending" => "bg-warning",
        "Locked" => "bg-danger",
        "Deleted" => "bg-secondary",
        _ => "bg-secondary"
    };

    public string RoleBadgeClass => Role switch
    {
        "SuperAdmin" => "bg-danger",
        "Admin" => "bg-warning text-dark",
        "User" => "bg-primary",
        _ => "bg-secondary"
    };
}

public class UserManagementFilters
{
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
}

public class UserManagementCapabilities
{
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanChangeRoles { get; set; }
    public bool CanLockUnlock { get; set; }
    public bool CanViewSensitiveData { get; set; }
    public bool CanExport { get; set; }
    public bool CanViewAllUsers { get; set; }
    public bool CanManageSuperAdmins { get; set; }
}