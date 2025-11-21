using FestivalLineupBySpotify_API.DTO;
using FestivalLineupBySpotify_API.Models;
using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            // Check if the user has a valid Spotify token cookie
            if (Request.Cookies.TryGetValue("AccessToken", out var token) && !string.IsNullOrEmpty(token))
            {
                return Ok(new { authenticated = true });
            }
            return Unauthorized(); 
        }


        [HttpGet]
        [Route("festival/{internalFestivalName}/{forceReloadData?}")]
        public async Task<ClashFindersFavoritesResponse> GenerateFavoritesForFestival(string internalFestivalName, bool forceReloadData = false)
        {
            var result = await _spotifyService.GenerateClashFindersFavoritesResult(Request, internalFestivalName, forceReloadData);
            return MapToResponse(result);
        }


        [HttpGet]
        [Route("festivals/{festivalsYear}")]
        public async Task<List<ClashFindersFavoritesResponse>> GenerateForFestivalsYear(int festivalsYear)
        {
            var results = await _spotifyService.GenerateClashFindersFavoritesResult(Request, festivalsYear);
            return results.Select(MapToResponse).ToList();
        }

        [HttpGet("festivals/list/all")]
        public async Task<List<FestivalListItemResponse>> GetAllFestivals()
        {
            return await _spotifyService.GetAllFestivals();
        }

        private static ClashFindersFavoritesResponse MapToResponse(ClashFindersFavoritesResult result)
        {
            var festivalResponse = result.Festival != null ? new FestivalResponse
            {
                Name = result.Festival.Name,
                Id = result.Festival.Id,
                Url = result.Festival.Url,
                PrintAdvisory = result.Festival.PrintAdvisory,
                Modified = result.Festival.Modified,
                StartDateUnix = result.Festival.StartDateUnix
            } : null;

            return new ClashFindersFavoritesResponse(result.Url, result.TotalPossibleLikedTracks, result.Rank, festivalResponse);
        }
    }    
}
