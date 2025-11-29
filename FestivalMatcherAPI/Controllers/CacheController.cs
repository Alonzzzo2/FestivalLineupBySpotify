using FestivalMatcherAPI.Configuration;
using FestivalMatcherAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FestivalMatcherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IClashFindersService _clashFindersService;
        private readonly string _expectedRefreshKey;

        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

        public CacheController(IClashFindersService clashFindersService, IOptions<CacheSettings> cacheSettings, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
        {
            _clashFindersService = clashFindersService;
            _expectedRefreshKey = cacheSettings.Value.RefreshKey;
            _cache = cache;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCache()
        {
            if (string.IsNullOrEmpty(_expectedRefreshKey))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Refresh key is not configured.");
            }

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || authHeader != $"Bearer {_expectedRefreshKey}")
            {
                return Unauthorized("Invalid or missing refresh key.");
            }

            await _clashFindersService.RefreshCacheAsync();
            return Ok("Cache refresh initiated.");
        }

        [HttpGet("debug")]
        public async Task<IActionResult> DebugRedis()
        {
            try
            {
                var key = "debug_key_" + Guid.NewGuid();
                var value = "debug_value_" + DateTime.UtcNow;
                
                await _cache.SetStringAsync(key, value, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) 
                });

                var retrieved = await _cache.GetStringAsync(key);

                if (retrieved == value)
                {
                    return Ok(new { Status = "Success", Message = "Redis is working!", Key = key, Value = retrieved });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Failure", Message = "Value mismatch.", Expected = value, Actual = retrieved });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
}
