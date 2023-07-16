using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Diagnostics;
using SpotifyAPI.Web;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SpotifyController : ControllerBase
    {
        private readonly ISpotifyService _spotifyService;

        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }
        
        [HttpGet(Name = "Test Sppotify API call")]
        public async Task<ClashFindersFavoritesResult> GeneratePersonalizedClashFindersURL(string festivalName)
        {
            var result = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalName);
            return result;
        }
    }

    
}
