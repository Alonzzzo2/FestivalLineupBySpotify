namespace FestivalLineupBySpotify_API.Services
{
    /// <summary>
    /// Service for handling Spotify OAuth authentication flows
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Generate the Spotify login URL for PKCE OAuth flow
        /// </summary>
        /// <returns>Spotify authorization URL</returns>
        string GenerateSpotifyLoginUrl();

        /// <summary>
        /// Exchange Spotify authorization code for access token
        /// </summary>
        /// <param name="code">Authorization code from Spotify</param>
        /// <param name="state">State parameter containing encoded PKCE verifier</param>
        /// <returns>Access token string</returns>
        Task<string> ExchangeCodeForAccessToken(string code, string state);

        /// <summary>
        /// Check if valid access token exists in cookies
        /// </summary>
        /// <param name="cookies">Request cookies collection</param>
        /// <returns>True if valid token exists, false otherwise</returns>
        bool HasValidAccessToken(IRequestCookieCollection cookies);
    }
}
