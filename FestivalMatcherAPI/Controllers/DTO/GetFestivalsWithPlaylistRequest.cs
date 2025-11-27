using FluentValidation;

namespace FestivalMatcherAPI.Controllers.DTO
{
    /// <summary>
    /// Request parameters for getting festivals matched with a public playlist
    /// </summary>
    public class GetFestivalsWithPlaylistRequest
    {
        /// <summary>
        /// Public Spotify playlist URL
        /// Must be a valid public Spotify playlist URL from open.spotify.com
        /// </summary>
        /// <example>https://open.spotify.com/playlist/3cEYpjA9oz9GiPac4AsQkj</example>
        public string PlaylistUrl { get; set; } = null!;
    }

    /// <summary>
    /// FluentValidation validator for GetFestivalsWithPlaylistRequest
    /// Validates that PlaylistUrl is a valid public Spotify playlist URL using regex pattern
    /// </summary>
    public class GetFestivalsWithPlaylistRequestValidator : AbstractValidator<GetFestivalsWithPlaylistRequest>
    {
        /// <summary>
        /// Regex pattern for valid Spotify public playlist URLs
        /// Pattern: https://open.spotify.com/playlist/{ID}
        /// Where ID is 10+ alphanumeric characters or underscores
        /// </summary>
        private const string SpotifyPlaylistUrlPattern = @"^https?://open\.spotify\.com/playlist/[a-zA-Z0-9_]{10,}$";

        public GetFestivalsWithPlaylistRequestValidator()
        {
            RuleFor(x => x.PlaylistUrl)
                .NotEmpty()
                .WithMessage("Playlist URL is required");

            RuleFor(x => x.PlaylistUrl)
                .Matches(SpotifyPlaylistUrlPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                .WithMessage("Invalid Spotify playlist URL. Use format: https://open.spotify.com/playlist/[playlist-id]");

            RuleFor(x => x.PlaylistUrl)
                .MaximumLength(2048)
                .WithMessage("Playlist URL must not exceed 2048 characters");
        }
    }
}
