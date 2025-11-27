using FestivalMatcherAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FestivalMatcherAPI.Controllers.DTO;

namespace FestivalMatcherAPI.Controllers
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
