using System.ComponentModel.DataAnnotations;

namespace FestivalMatcherAPI.Configuration
{
    /// <summary>
    /// Spotify OAuth configuration settings
    /// Required for authenticating with Spotify API
    /// </summary>
    public class SpotifySettings
    {
        /// <summary>
        /// Spotify application client ID
        /// Required for OAuth flow and public API access
        /// </summary>
        [Required(ErrorMessage = "Spotify ClientId is required.")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Spotify application client secret
        /// Required for client credentials authentication
        /// </summary>
        [Required(ErrorMessage = "Spotify ClientSecret is required.")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Spotify OAuth redirect URI
        /// Must match the URI registered in Spotify Developer Dashboard
        /// Required for OAuth callback handling
        /// </summary>
        [Required(ErrorMessage = "Spotify RedirectUri is required.")]
        public string RedirectUri { get; set; } = string.Empty;
    }
}
