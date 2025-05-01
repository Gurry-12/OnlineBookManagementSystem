using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly BookManagementContext _context;

        public ActivityLogger(BookManagementContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string actionType, string? description, int? userId = null)
        {
            var log = new ActivityLog
            {
                ActionType = actionType,
                Description = description,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

}
