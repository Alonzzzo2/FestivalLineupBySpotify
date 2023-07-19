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
        
        [HttpGet]
        [Route("festival/{festivalName}")]
        public async Task<ClashFindersFavoritesResult> GenerateForFestival(string festivalName)
        {
            var result = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalName);
            return result;
        }

        [HttpGet]
        [Route("festivals/{festivalsYear}")]
        public async Task<List<ClashFindersFavoritesResult>> GenerateForFestivalsYear(int festivalsYear)
        {
            var results = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalsYear);
            return results;
        }
    }    
}
