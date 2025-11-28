using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace FestivalMatcherAPI.Services.Caching
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetOrFetchAndSetAsync<T>(string key, Func<Task<T>> fetchFunc, DistributedCacheEntryOptions options) where T : class
        {
            var fromCache = await GetAsync<T>(key);
            if (fromCache != null)
            {
                return fromCache;
            }

            var data = await fetchFunc();            
            await SetAsync(key, data, options);            
            return data;
        }

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

        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null) where T : class
        {
            var json = JsonConvert.SerializeObject(value);
            options ??= new DistributedCacheEntryOptions();
            await _cache.SetStringAsync(key, json, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
