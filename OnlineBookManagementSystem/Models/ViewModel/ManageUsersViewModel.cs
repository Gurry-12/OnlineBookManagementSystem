namespace OnlineBookManagementSystem.Models.ViewModel;

public class ManageUsersViewModel
{
    public List<UserWithRoleViewModel> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }
    public string? SearchTerm { get; set; }
    public string? SelectedRole { get; set; }
    public string? SelectedStatus { get; set; }
}

public class UserWithRoleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool EmailConfirmed { get; set; }
}