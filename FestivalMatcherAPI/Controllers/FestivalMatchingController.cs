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

        /// <summary>
        /// Get all available festivals with user's liked songs matched to them
        /// </summary>
        /// <remarks>
        /// Requires Spotify authentication. Returns festivals for the specified year with user's liked tracks matched.
        /// Results are sorted by ranking (descending).
        /// </remarks>
        /// <param name="year">The festival year to filter by</param>
        /// <returns>List of festivals with matched artists and rankings</returns>
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

        /// <summary>
        /// Get user's liked songs matched to a specific festival
        /// </summary>
        /// <remarks>
        /// Requires Spotify authentication. Returns matching details for a single festival.
        /// </remarks>
        /// <param name="festivalId">The internal identifier of the festival (e.g., 'glastonbury', 'readingrocks')</param>
        /// <returns>ClashFinders link with matched artists and ranking</returns>
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

        #endregion

        #region Playlist Endpoints

        /// <summary>
        /// Get all available festivals with artists from a public playlist matched to them
        /// </summary>
        /// <remarks>
        /// No authentication required. Public playlist endpoint that matches any public Spotify playlist to all available festivals.
        /// Results are sorted by ranking (descending).
        /// Results are cached for 24 hours to optimize performance.
        /// </remarks>
        /// <param name="year">The festival year to filter by</param>
        /// <param name="request">Request containing the public playlist URL</param>
        /// <returns>List of festivals with playlist artists matched and rankings</returns>
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

        /// <summary>
        /// Get artists from a public playlist matched to a specific festival
        /// </summary>
        /// <remarks>
        /// No authentication required. Public playlist endpoint for matching a playlist to a single festival.
        /// Results are cached for 24 hours to optimize performance.
        /// </remarks>
        /// <param name="festivalId">The internal identifier of the festival</param>
        /// <param name="request">Request containing the public playlist URL</param>
        /// <returns>ClashFinders link with playlist artists matched to festival and ranking</returns>
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

        #endregion

        #region Response Mapping

        /// <summary>
        /// Maps internal ClashFindersLinkModel to public ClashFindersLinkResponse DTO
        /// </summary>
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
