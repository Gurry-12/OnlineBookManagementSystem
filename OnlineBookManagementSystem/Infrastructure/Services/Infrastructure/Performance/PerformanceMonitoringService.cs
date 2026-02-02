using System.Collections.Concurrent;
using System.Diagnostics;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance
{
    /// <summary>
    /// Service for monitoring and collecting performance metrics
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        void RecordRequestDuration(string endpoint, TimeSpan duration);
        void RecordDatabaseQueryDuration(string query, TimeSpan duration);
        void RecordCacheHit(string key);
        void RecordCacheMiss(string key);
        void RecordError(string operation, Exception exception);
        Task<PerformanceMetrics> GetMetricsAsync();
        Task<PerformanceReport> GenerateReportAsync();
        void StartOperation(string operationName);
        void EndOperation(string operationName);
    }

    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly ConcurrentDictionary<string, List<TimeSpan>> _requestDurations = new();
        private readonly ConcurrentDictionary<string, List<TimeSpan>> _queryDurations = new();
        private readonly ConcurrentDictionary<string, int> _cacheHits = new();
        private readonly ConcurrentDictionary<string, int> _cacheMisses = new();
        private readonly ConcurrentDictionary<string, int> _errorCounts = new();
        private readonly ConcurrentDictionary<string, Stopwatch> _activeOperations = new();
        private readonly object _lockObject = new object();

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
        }

        public void RecordRequestDuration(string endpoint, TimeSpan duration)
        {
            try
            {
                _requestDurations.AddOrUpdate(endpoint,
                    new List<TimeSpan> { duration },
                    (key, existing) =>
                    {
                        lock (_lockObject)
                        {
                            existing.Add(duration);
                            // Keep only last 100 measurements to prevent memory issues
                            if (existing.Count > 100)
                            {
                                existing.RemoveAt(0);
                            }
                            return existing;
                        }
                    });

                // Log slow requests
                if (duration.TotalSeconds > 2.0)
                {
                    _logger.LogWarning("Slow request detected: {Endpoint} took {Duration}ms",
                        endpoint, duration.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording request duration for {Endpoint}", endpoint);
            }
        }

        public void RecordDatabaseQueryDuration(string query, TimeSpan duration)
        {
            try
            {
                var queryKey = GetQueryKey(query);
                _queryDurations.AddOrUpdate(queryKey,
                    new List<TimeSpan> { duration },
                    (key, existing) =>
                    {
                        lock (_lockObject)
                        {
                            existing.Add(duration);
                            if (existing.Count > 50) // Smaller limit for queries
                            {
                                existing.RemoveAt(0);
                            }
                            return existing;
                        }
                    });

                // Log slow queries
                if (duration.TotalMilliseconds > 500)
                {
                    _logger.LogWarning("Slow database query detected: {Query} took {Duration}ms",
                        queryKey, duration.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording query duration");
            }
        }

        public void RecordCacheHit(string key)
        {
            try
            {
                _cacheHits.AddOrUpdate(key, 1, (k, v) => v + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit for {Key}", key);
            }
        }

        public void RecordCacheMiss(string key)
        {
            try
            {
                _cacheMisses.AddOrUpdate(key, 1, (k, v) => v + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache miss for {Key}", key);
            }
        }

        public void RecordError(string operation, Exception exception)
        {
            try
            {
                _errorCounts.AddOrUpdate(operation, 1, (k, v) => v + 1);
                _logger.LogError(exception, "Error in operation: {Operation}", operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording error for operation {Operation}", operation);
            }
        }

        public void StartOperation(string operationName)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _activeOperations.TryAdd(operationName, stopwatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting operation {Operation}", operationName);
            }
        }

        public void EndOperation(string operationName)
        {
            try
            {
                if (_activeOperations.TryRemove(operationName, out var stopwatch))
                {
                    stopwatch.Stop();
                    RecordRequestDuration(operationName, stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending operation {Operation}", operationName);
            }
        }

        public async Task<PerformanceMetrics> GetMetricsAsync()
        {
            try
            {
                await Task.CompletedTask; // For async consistency

                var metrics = new PerformanceMetrics
                {
                    RequestMetrics = CalculateRequestMetrics(),
                    DatabaseMetrics = CalculateDatabaseMetrics(),
                    CacheMetrics = CalculateCacheMetrics(),
                    ErrorMetrics = CalculateErrorMetrics(),
                    SystemMetrics = GetSystemMetrics(),
                    Timestamp = DateTime.UtcNow
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return new PerformanceMetrics { Timestamp = DateTime.UtcNow };
            }
        }

        public async Task<PerformanceReport> GenerateReportAsync()
        {
            try
            {
                var metrics = await GetMetricsAsync();

                var report = new PerformanceReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    Metrics = metrics,
                    Recommendations = GenerateRecommendations(metrics),
                    Summary = GenerateSummary(metrics)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                return new PerformanceReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    Summary = "Error generating performance report"
                };
            }
        }

        private RequestMetrics CalculateRequestMetrics()
        {
            var allDurations = _requestDurations.Values.SelectMany(list => list).ToList();

            if (!allDurations.Any())
            {
                return new RequestMetrics();
            }

            return new RequestMetrics
            {
                TotalRequests = allDurations.Count,
                AverageResponseTime = TimeSpan.FromMilliseconds(allDurations.Average(d => d.TotalMilliseconds)),
                MedianResponseTime = GetMedian(allDurations),
                P95ResponseTime = GetPercentile(allDurations, 95),
                P99ResponseTime = GetPercentile(allDurations, 99),
                SlowRequestCount = allDurations.Count(d => d.TotalSeconds > 2.0)
            };
        }

        private DatabaseMetrics CalculateDatabaseMetrics()
        {
            var allDurations = _queryDurations.Values.SelectMany(list => list).ToList();

            if (!allDurations.Any())
            {
                return new DatabaseMetrics();
            }

            return new DatabaseMetrics
            {
                TotalQueries = allDurations.Count,
                AverageQueryTime = TimeSpan.FromMilliseconds(allDurations.Average(d => d.TotalMilliseconds)),
                MedianQueryTime = GetMedian(allDurations),
                SlowQueryCount = allDurations.Count(d => d.TotalMilliseconds > 500),
                UniqueQueryCount = _queryDurations.Keys.Count
            };
        }

        private CacheMetrics CalculateCacheMetrics()
        {
            var totalHits = _cacheHits.Values.Sum();
            var totalMisses = _cacheMisses.Values.Sum();
            var totalRequests = totalHits + totalMisses;

            return new CacheMetrics
            {
                TotalHits = totalHits,
                TotalMisses = totalMisses,
                HitRate = totalRequests > 0 ? (double)totalHits / totalRequests * 100 : 0,
                MostAccessedKeys = _cacheHits.OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        private ErrorMetrics CalculateErrorMetrics()
        {
            return new ErrorMetrics
            {
                TotalErrors = _errorCounts.Values.Sum(),
                ErrorsByOperation = _errorCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                MostFrequentErrors = _errorCounts.OrderByDescending(kvp => kvp.Value)
                    .Take(5)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        private SystemMetrics GetSystemMetrics()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                return new SystemMetrics
                {
                    MemoryUsageMB = process.WorkingSet64 / (1024 * 1024),
                    CpuUsagePercent = GetCpuUsage(),
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    GCCollectionCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return new SystemMetrics();
            }
        }

        private double GetCpuUsage()
        {
            // Simplified CPU usage calculation
            // In production, you might want to use a more sophisticated approach
            try
            {
                var process = Process.GetCurrentProcess();
                return process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount / 1000.0;
            }
            catch
            {
                return 0;
            }
        }

        private TimeSpan GetMedian(List<TimeSpan> durations)
        {
            if (!durations.Any()) return TimeSpan.Zero;

            var sorted = durations.OrderBy(d => d.TotalMilliseconds).ToList();
            var mid = sorted.Count / 2;

            return sorted.Count % 2 == 0
                ? TimeSpan.FromMilliseconds((sorted[mid - 1].TotalMilliseconds + sorted[mid].TotalMilliseconds) / 2)
                : sorted[mid];
        }

        private TimeSpan GetPercentile(List<TimeSpan> durations, int percentile)
        {
            if (!durations.Any()) return TimeSpan.Zero;

            var sorted = durations.OrderBy(d => d.TotalMilliseconds).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            index = Math.Max(0, Math.Min(index, sorted.Count - 1));

            return sorted[index];
        }

        private string GetQueryKey(string query)
        {
            // Simplify query for grouping (remove parameters, etc.)
            if (string.IsNullOrEmpty(query)) return "unknown";

            // Take first 50 characters and remove parameters for grouping
            var simplified = query.Length > 50 ? query.Substring(0, 50) : query;
            return simplified.Split('?')[0]; // Remove query parameters
        }

        private List<string> GenerateRecommendations(PerformanceMetrics metrics)
        {
            var recommendations = new List<string>();

            if (metrics.RequestMetrics.AverageResponseTime.TotalSeconds > 1.0)
            {
                recommendations.Add("Consider implementing response caching for slow endpoints");
            }

            if (metrics.CacheMetrics.HitRate < 70)
            {
                recommendations.Add("Cache hit rate is low - review caching strategy");
            }

            if (metrics.DatabaseMetrics.SlowQueryCount > 0)
            {
                recommendations.Add("Optimize slow database queries or add indexes");
            }

            if (metrics.SystemMetrics.MemoryUsageMB > 500)
            {
                recommendations.Add("Memory usage is high - consider memory optimization");
            }

            if (metrics.ErrorMetrics.TotalErrors > 10)
            {
                recommendations.Add("Error rate is elevated - investigate error causes");
            }

            return recommendations;
        }

        private string GenerateSummary(PerformanceMetrics metrics)
        {
            return $"Performance Summary: {metrics.RequestMetrics.TotalRequests} requests processed, " +
                   $"average response time {metrics.RequestMetrics.AverageResponseTime.TotalMilliseconds:F0}ms, " +
                   $"cache hit rate {metrics.CacheMetrics.HitRate:F1}%, " +
                   $"{metrics.ErrorMetrics.TotalErrors} errors recorded.";
        }
    }

    // Data models for performance metrics
    public class PerformanceMetrics
    {
        public RequestMetrics RequestMetrics { get; set; } = new();
        public DatabaseMetrics DatabaseMetrics { get; set; } = new();
        public CacheMetrics CacheMetrics { get; set; } = new();
        public ErrorMetrics ErrorMetrics { get; set; } = new();
        public SystemMetrics SystemMetrics { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class RequestMetrics
    {
        public int TotalRequests { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan MedianResponseTime { get; set; }
        public TimeSpan P95ResponseTime { get; set; }
        public TimeSpan P99ResponseTime { get; set; }
        public int SlowRequestCount { get; set; }
    }

    public class DatabaseMetrics
    {
        public int TotalQueries { get; set; }
        public TimeSpan AverageQueryTime { get; set; }
        public TimeSpan MedianQueryTime { get; set; }
        public int SlowQueryCount { get; set; }
        public int UniqueQueryCount { get; set; }
    }

    public class CacheMetrics
    {
        public int TotalHits { get; set; }
        public int TotalMisses { get; set; }
        public double HitRate { get; set; }
        public Dictionary<string, int> MostAccessedKeys { get; set; } = new();
    }

    public class ErrorMetrics
    {
        public int TotalErrors { get; set; }
        public Dictionary<string, int> ErrorsByOperation { get; set; } = new();
        public Dictionary<string, int> MostFrequentErrors { get; set; } = new();
    }

    public class SystemMetrics
    {
        public long MemoryUsageMB { get; set; }
        public double CpuUsagePercent { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public int GCCollectionCount { get; set; }
    }

    public class PerformanceReport
    {
        public DateTime GeneratedAt { get; set; }
        public PerformanceMetrics Metrics { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}