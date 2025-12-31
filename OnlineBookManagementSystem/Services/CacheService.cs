using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace OnlineBookManagementSystem.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache memoryCache, IConfiguration configuration, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                if (value is string jsonString)
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString));
                }
                return Task.FromResult((T?)value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key}", key);
        }

        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }
            else
            {
                var defaultExpiration = _configuration.GetValue<int>("Caching:DefaultExpirationMinutes", 30);
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(defaultExpiration);
            }

            var slidingExpiration = _configuration.GetValue<int>("Caching:SlidingExpirationMinutes", 10);
            options.SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration);

            if (value is string)
            {
                _memoryCache.Set(key, value, options);
            }
            else
            {
                var jsonString = JsonSerializer.Serialize(value);
                _memoryCache.Set(key, jsonString, options);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        // Memory cache doesn't support pattern removal easily
        // This would be better implemented with Redis
        _logger.LogWarning("Pattern-based cache removal not implemented for MemoryCache");
        return Task.CompletedTask;
    }
}