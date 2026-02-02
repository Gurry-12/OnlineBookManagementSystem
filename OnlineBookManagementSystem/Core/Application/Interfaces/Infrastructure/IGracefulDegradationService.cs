namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance
{
    /// <summary>
    /// Service for handling graceful degradation and fallback scenarios
    /// </summary>
    public interface IGracefulDegradationService
    {
        /// <summary>
        /// Executes an operation with fallback support
        /// </summary>
        Task<T> ExecuteWithFallbackAsync<T>(
            Func<Task<T>> primaryOperation,
            Func<Task<T>> fallbackOperation,
            string operationName);

        /// <summary>
        /// Executes an operation with retry logic
        /// </summary>
        Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            TimeSpan? delay = null,
            string operationName = "");

        /// <summary>
        /// Executes an operation with timeout
        /// </summary>
        Task<T> ExecuteWithTimeoutAsync<T>(
            Func<Task<T>> operation,
            TimeSpan timeout,
            string operationName = "");

        /// <summary>
        /// Checks if a service is healthy
        /// </summary>
        Task<bool> IsServiceHealthyAsync(string serviceName);
    }
}