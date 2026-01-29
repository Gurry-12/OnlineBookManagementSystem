using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Activity;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Logging
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly BookManagementContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ActivityLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogger(BookManagementContext context, UserManager<User> userManager, ILogger<ActivityLogger> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string actionType, string? description, int? userId = null)
        {
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var context = _httpContextAccessor.HttpContext;
                var ipAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                var log = new ActivityLog
                {
                    Action = actionType,
                    Message = description,
                    Timestamp = indianTime,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity: {ActionType}", actionType);
            }
        }

        // Alias method for compatibility
        public async Task LogActivityAsync(string actionType, string? description, int? userId = null)
        {
            await LogAsync(actionType, description, userId);
        }

        public async Task<List<ActivityLog>> GetLogsAsync(int? userId = null)
        {
            var logs = await _context.ActivityLogs
                .Include(log => log.User)
                .Where(log => userId == null || log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
            return logs;
        }

        public async Task<List<ActivityLogViewModel>> GetAllLogsAsync()
        {
            // Fix: Remove Include() when using Select() projection to avoid EF Core tracking issues
            var activityModel = await _context.ActivityLogs
                .OrderByDescending(log => log.Timestamp)
                .Select(log => new ActivityLogViewModel
                {
                    ActionType = log.ActionType,
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    UserName = log.User != null ? log.User.UserName : "System",
                    Description = log.Description,
                    TimeAgo = log.Timestamp != null ?
                           (DateTime.Now - log.Timestamp).TotalMinutes < 1 ? "Just now" :
                           (DateTime.Now - log.Timestamp).TotalHours < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalMinutes} minutes ago" :
                           (DateTime.Now - log.Timestamp).TotalDays < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalHours} hours ago" :
                           $"{(int)(DateTime.Now - log.Timestamp).TotalDays} days ago"
                           : ""
                })
                .ToListAsync();
            return activityModel;
        }

        public async Task<List<ActivityLogViewModel>> GetTodayLogsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Fix: Remove Include() when using Select() projection to avoid EF Core tracking issues
            var activityModel = await _context.ActivityLogs
                .Where(log => log.Timestamp >= today && log.Timestamp < tomorrow)
                .OrderByDescending(log => log.Timestamp)
                .Select(log => new ActivityLogViewModel
                {
                    ActionType = log.ActionType,
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    UserName = log.User != null ? log.User.UserName : "System",
                    Description = log.Description,
                    TimeAgo = log.Timestamp != null ?
                           (DateTime.Now - log.Timestamp).TotalMinutes < 1 ? "Just now" :
                           (DateTime.Now - log.Timestamp).TotalHours < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalMinutes} minutes ago" :
                           (DateTime.Now - log.Timestamp).TotalDays < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalHours} hours ago" :
                           $"{(int)(DateTime.Now - log.Timestamp).TotalDays} days ago"
                           : ""
                })
                .ToListAsync();
            return activityModel;
        }

        public async Task<List<ActivityLogViewModel>> GetFilteredLogsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, string? search = null, string? actionType = null)
        {
            // Fix: Remove Include() when using Select() projection to avoid EF Core tracking issues
            var query = _context.ActivityLogs.AsQueryable();

            // Apply date filters
            if (dateFrom.HasValue)
            {
                query = query.Where(log => log.Timestamp >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(log => log.Timestamp <= dateTo.Value.AddDays(1));
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(log =>
                    log.Description.Contains(search) ||
                    log.ActionType.Contains(search) ||
                    (log.User != null && (log.User.Name.Contains(search) || log.User.Email.Contains(search))));
            }

            // Apply action type filter
            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(log => log.ActionType == actionType);
            }

            var activityModel = await query
                .OrderByDescending(log => log.Timestamp)
                .Select(log => new ActivityLogViewModel
                {
                    ActionType = log.ActionType,
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    UserName = log.User != null ? log.User.UserName : "System",
                    Description = log.Description,
                    TimeAgo = log.Timestamp != null ?
                           (DateTime.Now - log.Timestamp).TotalMinutes < 1 ? "Just now" :
                           (DateTime.Now - log.Timestamp).TotalHours < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalMinutes} minutes ago" :
                           (DateTime.Now - log.Timestamp).TotalDays < 1 ? $"{(int)(DateTime.Now - log.Timestamp).TotalHours} hours ago" :
                           $"{(int)(DateTime.Now - log.Timestamp).TotalDays} days ago"
                           : ""
                })
                .ToListAsync();
            return activityModel;
        }

        public async Task<ActivityLogsViewModel> GetActivityLogsAsync(int page, int pageSize, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool excludeSystemLogs = false)
        {
            var query = _context.ActivityLogs
                .Include(log => log.User)
                .AsQueryable();

            // Default to today's logs if no date filters are provided
            if (!dateFrom.HasValue && !dateTo.HasValue)
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                query = query.Where(log => log.Timestamp >= today && log.Timestamp < tomorrow);
            }
            else
            {
                // Apply custom date filters
                if (dateFrom.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(log => log.Timestamp <= dateTo.Value.AddDays(1));
                }
            }

            // Apply other filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(log =>
                    log.Description.Contains(search) ||
                    log.ActionType.Contains(search) ||
                    (log.User != null && (log.User.Name.Contains(search) || log.User.Email.Contains(search))));
            }

            if (excludeSystemLogs)
            {
                query = query.Where(log => log.UserId != null);
            }

            var totalLogs = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get statistics
            var todayLogs = await _context.ActivityLogs
                .CountAsync(log => log.Timestamp.Date == DateTime.Today);

            var activeUsers = await _context.ActivityLogs
                .Where(log => log.UserId != null && log.Timestamp > DateTime.UtcNow.AddHours(-24))
                .Select(log => log.UserId)
                .Distinct()
                .CountAsync();

            var errorLogs = await _context.ActivityLogs
                .CountAsync(log => log.ActionType.ToLower().Contains("error") || log.ActionType.ToLower().Contains("exception"));

            return new ActivityLogsViewModel
            {
                Logs = logs,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalLogs = totalLogs,
                TodayLogs = todayLogs,
                ActiveUsers = activeUsers,
                ErrorLogs = errorLogs,
                SearchTerm = search,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
        }

        public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int count, bool excludeSystemLogs = false)
        {
            var query = _context.ActivityLogs
                .Include(log => log.User)
                .AsQueryable();

            if (excludeSystemLogs)
            {
                query = query.Where(log => log.UserId != null);
            }

            return await query
                .OrderByDescending(log => log.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> ClearOldLogsAsync(int daysOld)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var oldLogs = await _context.ActivityLogs
                    .Where(log => log.Timestamp < cutoffDate)
                    .ToListAsync();

                if (oldLogs.Any())
                {
                    _context.ActivityLogs.RemoveRange(oldLogs);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cleared {Count} old activity logs older than {Days} days", oldLogs.Count, daysOld);
                    return oldLogs.Count;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear old logs");
                throw;
            }
        }
    }
}
