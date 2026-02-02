namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Caching
{
    /// <summary>
    /// Multi-level caching service interface providing memory and distributed caching
    /// </summary>
    public interface IMultiLevelCacheService
    {
        /// <summary>
        /// Gets a cached value or sets it using the provided factory function
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry);

        /// <summary>
        /// Tries to get a cached value
        /// </summary>
        bool TryGetValue<T>(string key, out T? value);

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        void Set<T>(string key, T value, TimeSpan expiry);

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Clears all cached values
        /// </summary>
        void Clear();
    }
}