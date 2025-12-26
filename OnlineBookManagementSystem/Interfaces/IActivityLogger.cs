using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IActivityLogger
    {
        Task LogAsync(string actionType, string? description, int? userId = null);
        Task<List<ActivityLog>> GetLogsAsync(int? userId = null);
        Task<List<ActivityLogViewModel>> GetAllLogsAsync();
        Task<ActivityLogsViewModel> GetActivityLogsAsync(int page, int pageSize, string? search = null, string? action = null, string? role = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool excludeSystemLogs = false);
        Task<List<ActivityLog>> GetRecentActivitiesAsync(int count, bool excludeSystemLogs = false);
        Task<int> ClearOldLogsAsync(int daysOld);
    }
}
