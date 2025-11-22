using FestivalLineupBySpotify_API.Configuration;
using FestivalLineupBySpotify_API.Constants;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;

namespace FestivalLineupBySpotify_API.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IOptions<SpotifySettings> _spotifySettings;
        private static readonly string[] OAuthScopes = 
        {
            Scopes.PlaylistReadPrivate,
            Scopes.PlaylistReadCollaborative,
            Scopes.UserLibraryRead
        };

        public AuthenticationService(IOptions<SpotifySettings> spotifySettings)
        {
            _spotifySettings = spotifySettings;
        }

        public string GenerateSpotifyLoginUrl()
        {
            var settings = _spotifySettings.Value;
            var generatedCode = PKCEUtil.GenerateCodes();
            var challenge = generatedCode.challenge;

            // Encode verifier in state parameter (will be returned by Spotify)
            var state = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(generatedCode.verifier));

            var loginRequest = new LoginRequest(
                new Uri(settings.RedirectUri),
                settings.ClientId,
                LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = OAuthScopes,
                State = state
            };

            return loginRequest.ToUri().ToString();
        }

        public async Task<string> ExchangeCodeForAccessToken(string code, string state)
        {
            if (string.IsNullOrEmpty(state))
                throw new InvalidOperationException("State parameter missing. Please start authentication again.");

            string verifier;
            try
            {
                verifier = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid state parameter. Please start authentication again.");
            }

            var settings = _spotifySettings.Value;
            var tokenResponse = await new OAuthClient().RequestToken(
                new PKCETokenRequest(
                    settings.ClientId,
                    code,
                    new Uri(settings.RedirectUri),
                    verifier)
            );

            return tokenResponse.AccessToken;
        }

        public bool HasValidAccessToken(IRequestCookieCollection cookies)
        {
            return cookies.TryGetValue(CookieNames.SpotifyAccessToken, out var token) 
                && !string.IsNullOrEmpty(token);
        }
    }
}
