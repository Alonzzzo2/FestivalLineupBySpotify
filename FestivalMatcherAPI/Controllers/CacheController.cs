using FestivalMatcherAPI.Configuration;
using FestivalMatcherAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace FestivalMatcherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IClashFindersService _clashFindersService;
        private readonly string _expectedRefreshKey;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheController> _logger;

        public CacheController(
            IClashFindersService clashFindersService, 
            IOptions<CacheSettings> cacheSettings, 
            IDistributedCache cache,
            ILogger<CacheController> logger)
        {
            _clashFindersService = clashFindersService;
            _expectedRefreshKey = cacheSettings.Value.RefreshKey;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCache()
        {
            _logger.LogInformation("Received cache refresh request");

            if (string.IsNullOrEmpty(_expectedRefreshKey))
            {
                _logger.LogError("Refresh key is not configured in settings");
                return StatusCode(StatusCodes.Status500InternalServerError, "Refresh key is not configured.");
            }

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || authHeader != $"Bearer {_expectedRefreshKey}")
            {
                _logger.LogWarning("Invalid or missing refresh key provided in Authorization header");
                return Unauthorized("Invalid or missing refresh key.");
            }

            _logger.LogInformation("Starting cache refresh via ClashFindersService");
            await _clashFindersService.RefreshCacheAsync();
            _logger.LogInformation("Cache refresh initiated successfully");
            return Ok("Cache refresh initiated.");
        }

        [HttpGet("debug")]
        public async Task<IActionResult> DebugRedis()
        {
            _logger.LogInformation("Starting Redis debug check");
            try
            {
                var key = "debug_key_" + Guid.NewGuid();
                var value = "debug_value_" + DateTime.UtcNow;
                
                await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) 
                });

                var retrieved = await _cache.GetStringAsync(key);

                if (retrieved == value)
                {
                    _logger.LogInformation("Redis debug check SUCCESS. Key: {Key}", key);
                    return Ok(new { Status = "Success", Message = "Redis is working!", Key = key, Value = retrieved });
                }
                else
                {
                    _logger.LogError("Redis debug check FAILED. Value mismatch. Expected: {Expected}, Actual: {Actual}", value, retrieved);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Failure", Message = "Value mismatch.", Expected = value, Actual = retrieved });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis debug check ERROR: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
}
