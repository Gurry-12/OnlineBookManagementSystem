namespace OnlineBookManagementSystem.Presentation.ViewModels.Activity;

public class ActivityLogViewModel
{
    public string Action { get; set; } = null!;
    public string ActionType { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
    public string TimeAgo { get; set; } = "";
}