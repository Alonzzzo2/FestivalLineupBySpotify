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
        /// Validate if a token string is non-empty
        /// </summary>
        /// <param name="token">The token string to validate</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        bool IsTokenValid(string token);
    }
}
