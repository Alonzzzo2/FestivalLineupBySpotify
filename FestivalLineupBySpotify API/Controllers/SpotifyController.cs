using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Diagnostics;
using SpotifyAPI.Web;
using Spotify_Alonzzo_API.Services;

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
        [Route("festival/{festivalName}/{forceReloadData}")]
        public async Task<ClashFindersFavoritesResult> GenerateFavoritesForFestival(string festivalName, bool forceReloadData = false)
        {
            var result = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalName, forceReloadData);
            return result;
        }

        [HttpGet]
        [Route("festival/{festivalName}/popular")]
        public async Task<ClashFindersFavoritesResult> GeneratePopularForFestival(string festivalName, bool forceReloadData = false)
        {
            var result = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalName, forceReloadData);
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
