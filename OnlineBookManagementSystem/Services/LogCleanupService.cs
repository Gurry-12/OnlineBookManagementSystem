using OnlineBookManagementSystem.Models;
using Microsoft.Extensions.Logging;

namespace OnlineBookManagementSystem.Services
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupService> _logger;  // Inject the logger

        public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;  // Initialize the logger
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupOldLoginLogsAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Runs every 24 hours
            }
        }

        private async Task CleanupOldLoginLogsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookManagementContext>();

            // Get the Indian Standard Time zone
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Calculate the cutoff time: 1 day ago in IST
            var cutoff = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(-1), indiaTimeZone);

            // Query to get the logs older than 1 day
            var oldLogs = dbContext.ActivityLogs
                .Where(log => log.ActionType == "Login" && log.Timestamp < cutoff);

            // Remove old login logs
            dbContext.ActivityLogs.RemoveRange(oldLogs);
            await dbContext.SaveChangesAsync();

            // Log the cleanup activity (Optional for debugging or tracking)
            _logger.LogInformation("Deleted old login logs older than 1 day as of {time}", DateTime.UtcNow);
        }
    }
}
