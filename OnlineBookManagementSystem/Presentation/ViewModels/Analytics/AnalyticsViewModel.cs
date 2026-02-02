namespace OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

public class AnalyticsViewModel<T>
{
    public T Data { get; set; } = default!;
    public AnalyticsFilters Filters { get; set; } = new();
    public AnalyticsCapabilities Capabilities { get; set; } = new();
}

public class AnalyticsFilters
{
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
    public DateTime EndDate { get; set; } = DateTime.Now;
    public string Period { get; set; } = "Last 30 Days";
    public string? Category { get; set; }
    public string? Status { get; set; }
}

public class AnalyticsCapabilities
{
    public bool CanView { get; set; }
    public bool CanExport { get; set; }
    public bool CanViewSensitiveMetrics { get; set; }
}