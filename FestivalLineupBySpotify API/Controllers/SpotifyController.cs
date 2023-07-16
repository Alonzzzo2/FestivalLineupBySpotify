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
        public async Task<string> GeneratePersonalizedClashFindersURL(string festivalName)
        {
            var result = await SpotifyService.GenerateClashFindersFavoritesResult(Request, festivalName);
            return result.Url;
        }
    }

    
}
