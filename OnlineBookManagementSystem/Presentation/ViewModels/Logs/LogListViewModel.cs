using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Logs;

public class LogListViewModel
{
    public List<ActivityLog> Logs { get; set; } = new();
    public LogFilters Filters { get; set; } = new();
    public LogCapabilities Capabilities { get; set; } = new();

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalLogs { get; set; }
    public int PageSize { get; set; } = 50;

    // Basic stats (not analytics)
    public int LogsToday { get; set; }
    public int LogsThisWeek { get; set; }
}

public class LogFilters
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SearchTerm { get; set; }
    public string? ActionType { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
}

public class LogCapabilities
{
    public bool CanView { get; set; }
    public bool CanViewAllUsersLogs { get; set; }
    public bool CanExport { get; set; }
    public bool CanViewSensitiveData { get; set; }
}