using Microsoft.Extensions.Caching.Distributed;

namespace FestivalMatcherAPI.Services.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;

        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null) where T : class;

        Task<T?> GetOrFetchAndSetAsync<T>(string key, Func<Task<T>> fetchFunc, DistributedCacheEntryOptions options) where T : class;

        Task RemoveAsync(string key);
    }
}
