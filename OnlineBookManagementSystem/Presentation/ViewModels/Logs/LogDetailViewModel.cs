using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Logs;

public class LogDetailViewModel
{
    public ActivityLog Log { get; set; } = null!;
    public LogCapabilities Capabilities { get; set; } = new();

    // Related logs (same user, same action type, etc.)
    public List<ActivityLog> RelatedLogs { get; set; } = new();
}