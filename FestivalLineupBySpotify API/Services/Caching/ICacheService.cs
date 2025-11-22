using Microsoft.Extensions.Caching.Distributed;

namespace FestivalLineupBySpotify_API.Services.Caching
{
    /// <summary>
    /// Generic distributed caching service interface for serializing and deserializing cached objects
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieves a cached value by key and deserializes it to the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The deserialized cached value or null if not found</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Serializes and caches a value with the specified key
        /// </summary>
        /// <typeparam name="T">The type of the value being cached</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="options">Cache entry options (TTL, expiration, etc.)</param>
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null) where T : class;

        /// <summary>
        /// Removes a cached entry by key
        /// </summary>
        /// <param name="key">The cache key</param>
        Task RemoveAsync(string key);
    }
}
