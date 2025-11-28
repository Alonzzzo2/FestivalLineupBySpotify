using FestivalMatcherAPI.Controllers.DTO;
using FestivalMatcherAPI.Models;
using FestivalMatcherAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FestivalMatcherAPI.Controllers
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

        #region Liked Songs Endpoints

        [HttpGet("{festivalId}")]
        [ProducesResponseType(typeof(ClashFindersLinkResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClashFindersLinkResponse>> GetFestivalWithLikedSongs(
            string festivalId)
        {
            var result = await _festivalMatchingService.GetMatchedFestivalByNameForLikedSongs(festivalId);
            return Ok(MapToResponse(result));
        }

        [HttpGet("by-year/{year}")]
        [ProducesResponseType(typeof(List<ClashFindersLinkResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ClashFindersLinkResponse>>> GetFestivalsByYearWithLikedSongs(
            int year)
        {
            var results = await _festivalMatchingService.GetMatchedFestivalsByYearForLikedSongs(year);
            return Ok(results.Select(MapToResponse).ToList());
        }

        #endregion

        #region Playlist Endpoints

        [HttpGet("{festivalId}/playlist")]
        [ProducesResponseType(typeof(ClashFindersLinkResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClashFindersLinkResponse>> GetFestivalWithPlaylist(
            string festivalId,
            [FromQuery] GetFestivalsWithPlaylistRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _festivalMatchingService.GetMatchedFestivalByNameForPlaylist(
                request.PlaylistUrl,
                festivalId);

            return Ok(MapToResponse(result));
        }

        [HttpGet("by-year/{year}/playlist")]
        [ProducesResponseType(typeof(List<ClashFindersLinkResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ClashFindersLinkResponse>>> GetFestivalsByYearWithPlaylist(
            int year,
            [FromQuery] GetFestivalsWithPlaylistRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var results = await _festivalMatchingService.GetMatchedFestivalsByYearForPlaylist(
                request.PlaylistUrl,
                year);

            return Ok(results.Select(MapToResponse).ToList());
        }

        #endregion

        #region Response Mapping

        private static ClashFindersLinkResponse MapToResponse(ClashFindersLinkModel model)
        {
            var festivalResponse = model.Festival != null ? new FestivalResponse
            {
                Name = model.Festival.Name,
                Id = model.Festival.Id,
                Url = model.Festival.Url,
                StartDate = model.Festival.StartDate
            } : null;

            return new ClashFindersLinkResponse(
                model.Url,
                model.MatchedArtistsCount,
                model.MatchedTracks,
                model.TracksPerShow,
                model.RankingMessage,
                festivalResponse);
        }

        #endregion
    }
}
