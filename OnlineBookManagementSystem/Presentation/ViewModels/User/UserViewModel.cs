namespace OnlineBookManagementSystem.Presentation.ViewModels.User;

public class UserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CartItemCount { get; set; }
    public List<string> Roles { get; internal set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }

    // Additional properties
    public string? PhoneNumber { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
}