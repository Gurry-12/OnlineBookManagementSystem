using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Activity;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging
{
    public interface IActivityLogger
    {
        Task LogAsync(string actionType, string? description, int? userId = null);
        Task LogActivityAsync(string actionType, string? description, int? userId = null); // Alias for compatibility
        Task<List<ActivityLog>> GetLogsAsync(int? userId = null);
        Task<List<ActivityLogViewModel>> GetAllLogsAsync();
        Task<List<ActivityLogViewModel>> GetTodayLogsAsync();
        Task<List<ActivityLogViewModel>> GetFilteredLogsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, string? search = null, string? actionType = null);
        Task<ActivityLogsViewModel> GetActivityLogsAsync(int page, int pageSize, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool excludeSystemLogs = false);
        Task<List<ActivityLog>> GetRecentActivitiesAsync(int count, bool excludeSystemLogs = false);
        Task<int> ClearOldLogsAsync(int daysOld);
    }
}
