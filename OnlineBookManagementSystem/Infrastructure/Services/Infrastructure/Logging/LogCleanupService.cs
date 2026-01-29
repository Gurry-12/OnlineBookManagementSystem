using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Logging
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Activity Log Cleanup Service started");

            // Wait for 1 minute after startup before first execution
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldActivityLogsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up activity logs");
                }

                // Wait 24 hours before next cleanup
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupOldActivityLogsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var activityLogger = scope.ServiceProvider.GetRequiredService<IActivityLogger>();

                // Delete logs older than 1 day
                var deletedCount = await activityLogger.ClearOldLogsAsync(1);

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Successfully deleted {Count} activity logs older than 1 day", deletedCount);
                    
                    // Log this cleanup action (but don't create an infinite loop by logging the cleanup of cleanup logs)
                    await activityLogger.LogAsync("AutoCleanup", $"Automatically deleted {deletedCount} old activity logs", null);
                }
                else
                {
                    _logger.LogDebug("No old activity logs found to delete");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old activity logs");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Activity Log Cleanup Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
