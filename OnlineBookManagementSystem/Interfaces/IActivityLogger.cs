using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IActivityLogger
    {
        Task LogAsync(string actionType, string? description, int? userId = null);
            Task<List<ActivityLog>> GetLogsAsync(int? userId = null);

        Task<List<ActivityLog>> GetAllLogsAsync();



    }

}
