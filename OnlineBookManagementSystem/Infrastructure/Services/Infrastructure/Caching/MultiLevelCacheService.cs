using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Caching
{
    /// <summary>
    /// Multi-level caching implementation with memory and distributed cache
    /// </summary>
    public class MultiLevelCacheService : IMultiLevelCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<MultiLevelCacheService> _logger;

        // Default cache durations
        private readonly TimeSpan _defaultMemoryCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _defaultDistributedCacheDuration = TimeSpan.FromHours(1);

        public MultiLevelCacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<MultiLevelCacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry)
        {
            try
            {
                // L1: Check memory cache first (fastest)
                if (_memoryCache.TryGetValue(key, out T? memoryValue) && memoryValue != null)
                {
                    _logger.LogDebug("Cache hit (Memory): {Key}", key);
                    return memoryValue;
                }

                // L2: Check distributed cache (shared, persistent)
                var distributedValue = await GetFromDistributedCacheAsync<T>(key);
                if (distributedValue != null)
                {
                    _logger.LogDebug("Cache hit (Distributed): {Key}", key);
                    
                    // Store in memory cache for faster subsequent access
                    var memoryCacheDuration = expiry > _defaultMemoryCacheDuration ? _defaultMemoryCacheDuration : expiry;
                    _memoryCache.Set(key, distributedValue, memoryCacheDuration);
                    
                    return distributedValue;
                }

                // L3: Generate value using factory (most expensive)
                _logger.LogDebug("Cache miss, generating value: {Key}", key);
                var freshValue = await factory();

                if (freshValue != null)
                {
                    // Store in both caches
                    await SetInDistributedCacheAsync(key, freshValue, expiry);
                    
                    var memoryCacheDuration = expiry > _defaultMemoryCacheDuration ? _defaultMemoryCacheDuration : expiry;
                    _memoryCache.Set(key, freshValue, memoryCacheDuration);
                }

                return freshValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);
                
                // Fallback to factory method
                try
                {
                    return await factory();
                }
                catch (Exception factoryEx)
                {
                    _logger.LogError(factoryEx, "Factory method also failed for key: {Key}", key);
                    throw;
                }
            }
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            try
            {
                // Check memory cache first
                if (_memoryCache.TryGetValue(key, out value) && value != null)
                {
                    _logger.LogDebug("Cache hit (Memory): {Key}", key);
                    return true;
                }

                // Check distributed cache (synchronous version)
                var distributedValue = GetFromDistributedCacheSync<T>(key);
                if (distributedValue != null)
                {
                    _logger.LogDebug("Cache hit (Distributed): {Key}", key);
                    
                    // Store in memory cache
                    _memoryCache.Set(key, distributedValue, _defaultMemoryCacheDuration);
                    value = distributedValue;
                    return true;
                }

                value = default;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetValue for key: {Key}", key);
                value = default;
                return false;
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            try
            {
                if (value != null)
                {
                    // Set in memory cache
                    var memoryCacheDuration = expiry > _defaultMemoryCacheDuration ? _defaultMemoryCacheDuration : expiry;
                    _memoryCache.Set(key, value, memoryCacheDuration);

                    // Set in distributed cache (fire and forget)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SetInDistributedCacheAsync(key, value, expiry);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to set distributed cache for key: {Key}", key);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Set for key: {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                // Remove from memory cache
                _memoryCache.Remove(key);

                // Remove from distributed cache (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.RemoveAsync(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to remove from distributed cache for key: {Key}", key);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Remove for key: {Key}", key);
            }
        }

        public void Clear()
        {
            try
            {
                // Clear memory cache
                if (_memoryCache is MemoryCache memCache)
                {
                    memCache.Clear();
                }

                _logger.LogInformation("Cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }

        private async Task<T?> GetFromDistributedCacheAsync<T>(string key)
        {
            try
            {
                var cachedBytes = await _distributedCache.GetAsync(key);
                if (cachedBytes != null)
                {
                    var cachedString = Encoding.UTF8.GetString(cachedBytes);
                    return JsonSerializer.Deserialize<T>(cachedString);
                }
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting from distributed cache for key: {Key}", key);
                return default;
            }
        }

        private T? GetFromDistributedCacheSync<T>(string key)
        {
            try
            {
                var cachedBytes = _distributedCache.Get(key);
                if (cachedBytes != null)
                {
                    var cachedString = Encoding.UTF8.GetString(cachedBytes);
                    return JsonSerializer.Deserialize<T>(cachedString);
                }
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting from distributed cache (sync) for key: {Key}", key);
                return default;
            }
        }

        private async Task SetInDistributedCacheAsync<T>(string key, T value, TimeSpan expiry)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var cachedBytes = Encoding.UTF8.GetBytes(serializedValue);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };

                await _distributedCache.SetAsync(key, cachedBytes, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting distributed cache for key: {Key}", key);
            }
        }
    }
}