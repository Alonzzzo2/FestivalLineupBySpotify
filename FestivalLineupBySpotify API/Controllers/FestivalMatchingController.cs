using Spotify_Alonzzo_API.Controllers.DTO;
using FestivalLineupBySpotify_API.Models;
using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FestivalMatchingController : ControllerBase
    {
        private readonly IFestivalMatchingService _festivalMatchingService;

        public FestivalMatchingController(IFestivalMatchingService festivalMatchingService)
        {
            _festivalMatchingService = festivalMatchingService;
        }

        /// <summary>
        /// Get all available festivals with user's favorite artists matched to them
        /// </summary>
        /// <param name="year">The festival year to filter by</param>
        /// <param name="forceReloadData">Force reload user's favorite artists from Spotify (bypass cache)</param>
        /// <returns>List of festivals with matched artists and rankings, sorted by rank (descending)</returns>
        [HttpGet("by-year/{year}")]
        public async Task<List<ClashFindersFavoritesResponse>> GetMatchedFestivalsByYear(int year, [FromQuery] bool forceReloadData = false)
        {
            var results = await _festivalMatchingService.GetMatchedFestivalsByYear(year, forceReloadData);
            return results.Select(MapToResponse).ToList();
        }

        /// <summary>
        /// Get user's favorite artists matched to a specific festival
        /// </summary>
        /// <param name="internalFestivalName">The internal identifier of the festival</param>
        /// <param name="forceReloadData">Force reload user's favorite artists from Spotify (bypass cache)</param>
        /// <returns>ClashFinders link with matched artists and ranking</returns>
        [HttpGet("{internalFestivalName}")]
        public async Task<ClashFindersFavoritesResponse> GetMatchedFestivalByName(string internalFestivalName, [FromQuery] bool forceReloadData = false)
        {
            var result = await _festivalMatchingService.GetMatchedFestivalByName(internalFestivalName, forceReloadData);
            return MapToResponse(result);
        }

        private static ClashFindersFavoritesResponse MapToResponse(ClashFindersLinkModel model)
        {
            var festivalResponse = model.Festival != null ? new FestivalResponse
            {
                Name = model.Festival.Name,
                Id = model.Festival.Id,
                Url = model.Festival.Url,
                PrintAdvisory = model.Festival.PrintAdvisory,
                Modified = model.Festival.Modified,
                StartDateUnix = model.Festival.StartDateUnix
            } : null;

            return new ClashFindersFavoritesResponse(model.Url, model.TotalPossibleLikedTracks, model.Rank, festivalResponse);
        }
    }
}
