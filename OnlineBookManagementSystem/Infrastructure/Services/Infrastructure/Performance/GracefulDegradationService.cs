using System.Collections.Concurrent;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance
{
    /// <summary>
    /// Service for implementing graceful degradation patterns when services fail
    /// </summary>
    public class GracefulDegradationService : IGracefulDegradationService
    {
        private readonly ILogger<GracefulDegradationService> _logger;
        private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();
        private readonly ConcurrentDictionary<string, ServiceHealthInfo> _serviceHealth = new();

        public GracefulDegradationService(ILogger<GracefulDegradationService> logger)
        {
            _logger = logger;
        }

        public async Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> primaryOperation, Func<Task<T>> fallbackOperation, string operationName)
        {
            try
            {
                _logger.LogDebug("Executing primary operation: {OperationName}", operationName);
                var result = await primaryOperation();
                RecordServiceSuccess(operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary operation failed, executing fallback: {OperationName}", operationName);
                RecordServiceFailure(operationName);

                try
                {
                    var fallbackResult = await fallbackOperation();
                    _logger.LogInformation("Fallback operation succeeded: {OperationName}", operationName);
                    return fallbackResult;
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Both primary and fallback operations failed: {OperationName}", operationName);
                    throw new AggregateException("Both primary and fallback operations failed", ex, fallbackEx);
                }
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, TimeSpan? delay = null, string operationName = "")
        {
            var retryDelay = delay ?? TimeSpan.FromMilliseconds(500);
            Exception? lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        _logger.LogDebug("Retry attempt {Attempt} for operation: {OperationName}", attempt, operationName);
                        await Task.Delay(retryDelay * attempt); // Exponential backoff
                    }

                    var result = await operation();

                    if (attempt > 0)
                    {
                        _logger.LogInformation("Operation succeeded on retry attempt {Attempt}: {OperationName}", attempt, operationName);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt == maxRetries)
                    {
                        _logger.LogError(ex, "Operation failed after {MaxRetries} retries: {OperationName}", maxRetries, operationName);
                        break;
                    }

                    _logger.LogWarning(ex, "Operation failed on attempt {Attempt}, will retry: {OperationName}", attempt + 1, operationName);
                }
            }

            throw lastException ?? new InvalidOperationException("Operation failed with unknown error");
        }

        public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName = "")
        {
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                var result = await operation();
                return result;
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("Operation timed out after {Timeout}: {OperationName}", timeout, operationName);
                throw new TimeoutException($"Operation '{operationName}' timed out after {timeout}");
            }
        }

        public async Task<bool> IsServiceHealthyAsync(string serviceName)
        {
            await Task.CompletedTask; // For async consistency

            if (!_serviceHealth.TryGetValue(serviceName, out var healthInfo))
                return true; // Assume healthy if no data

            var circuitBreaker = _circuitBreakers.GetOrAdd(serviceName, _ => new CircuitBreakerState());

            return circuitBreaker.State != CircuitState.Open &&
                   healthInfo.SuccessRate > 0.7 && // 70% success rate threshold
                   healthInfo.LastSuccessTime > DateTime.UtcNow.AddMinutes(-5); // Recent success
        }

        private void RecordServiceFailure(string serviceName)
        {
            var healthInfo = _serviceHealth.GetOrAdd(serviceName, _ => new ServiceHealthInfo());

            lock (healthInfo)
            {
                healthInfo.TotalRequests++;
                healthInfo.FailureCount++;
                healthInfo.LastFailureTime = DateTime.UtcNow;
                healthInfo.SuccessRate = (double)(healthInfo.TotalRequests - healthInfo.FailureCount) / healthInfo.TotalRequests;
            }

            _logger.LogDebug("Recorded failure for service {ServiceName}. Success rate: {SuccessRate:P}",
                serviceName, healthInfo.SuccessRate);
        }

        private void RecordServiceSuccess(string serviceName)
        {
            var healthInfo = _serviceHealth.GetOrAdd(serviceName, _ => new ServiceHealthInfo());

            lock (healthInfo)
            {
                healthInfo.TotalRequests++;
                healthInfo.LastSuccessTime = DateTime.UtcNow;
                healthInfo.SuccessRate = (double)(healthInfo.TotalRequests - healthInfo.FailureCount) / healthInfo.TotalRequests;
            }

            _logger.LogDebug("Recorded success for service {ServiceName}. Success rate: {SuccessRate:P}",
                serviceName, healthInfo.SuccessRate);
        }
    }

    // Supporting classes and enums
    public class CircuitBreakerState
    {
        public CircuitState State { get; set; } = CircuitState.Closed;
        public int FailureCount { get; set; }
        public int FailureThreshold { get; set; } = 5;
        public TimeSpan ResetTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public DateTime NextAttemptTime { get; set; }
    }

    public enum CircuitState
    {
        Closed,   // Normal operation
        Open,     // Failing, rejecting requests
        HalfOpen  // Testing if service has recovered
    }

    public class ServiceHealthInfo
    {
        public int TotalRequests { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate { get; set; } = 1.0;
        public DateTime LastSuccessTime { get; set; } = DateTime.UtcNow;
        public DateTime LastFailureTime { get; set; }
    }
}