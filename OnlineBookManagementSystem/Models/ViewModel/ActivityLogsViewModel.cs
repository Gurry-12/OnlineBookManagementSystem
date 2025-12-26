namespace OnlineBookManagementSystem.Models.ViewModel;

public class ActivityLogsViewModel
{
    public List<ActivityLog> Logs { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalLogs { get; set; }
    public int TodayLogs { get; set; }
    public int ActiveUsers { get; set; }
    public int ErrorLogs { get; set; }
    
    // Filter properties
    public string? SearchTerm { get; set; }
    public string? ActionFilter { get; set; }
    public string? RoleFilter { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}