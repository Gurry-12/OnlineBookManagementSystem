using Microsoft.EntityFrameworkCore;
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
            var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

            var log = new ActivityLog
            {
                ActionType = actionType,
                Description = description,
                Timestamp = indianTime,  // Set the timestamp to IST
                UserId = userId
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetLogsAsync(int? userId = null)
        {
            var logs = await _context.ActivityLogs
                .Where(log => userId == null || log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
            return logs;
        }

        public async Task<List<ActivityLog>> GetAllLogsAsync()
        {
            return await _context.ActivityLogs
                                 .OrderByDescending(log => log.Timestamp)
                                 .ToListAsync();
        }


    }

}
