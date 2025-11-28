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

        public CacheController(IClashFindersService clashFindersService, IOptions<CacheSettings> cacheSettings)
        {
            _clashFindersService = clashFindersService;
            _expectedRefreshKey = cacheSettings.Value.RefreshKey;
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
    }
}
