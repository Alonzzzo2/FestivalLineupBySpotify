using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Mvc;
using Spotify_Alonzzo_API.Controllers.DTO;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClashFindersController : ControllerBase
    {
        private readonly IClashFindersService _clashFindersService;

        public ClashFindersController(IClashFindersService clashFindersService)
        {
            _clashFindersService = clashFindersService;
        }

        /// <summary>
        /// Get all available festivals
        /// </summary>
        /// <returns>List of all festivals with basic information</returns>
        [HttpGet("list/all")]
        public async Task<List<FestivalListItemResponse>> GetAllFestivals()
        {
            var festivals = await _clashFindersService.GetAllFestivals();
            return festivals.Select(f => new FestivalListItemResponse
            {
                Title = f.Title,
                InternalName = f.InternalName,
                StartDate = f.StartDate,
                PrintAdvisory = f.PrintAdvisory
            }).ToList();
        }
    }
}
