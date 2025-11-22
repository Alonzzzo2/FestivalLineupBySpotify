using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace FestivalLineupBySpotify_API.Services.Caching
{
    /// <summary>
    /// Implementation of ICacheService using IDistributedCache with JSON serialization
    /// </summary>
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a cached value by key and deserializes it from JSON
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedJson = await _cache.GetStringAsync(key);
                if (cachedJson == null)
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(cachedJson);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Serializes a value to JSON and caches it with the specified options
        /// </summary>
        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null) where T : class
        {
            var json = JsonConvert.SerializeObject(value);
            options ??= new DistributedCacheEntryOptions();
            await _cache.SetStringAsync(key, json, options);
        }

        /// <summary>
        /// Removes a cached entry by key
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
