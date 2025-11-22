using FestivalLineupBySpotify_API.Constants;
using FestivalLineupBySpotify_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private const string FrontendUrlConfigKey = "FrontendUrl";
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private static readonly TimeSpan TokenExpiration = TimeSpan.FromHours(1);

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Check if the user is authenticated with Spotify
        /// </summary>
        /// <returns>Authentication status</returns>
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            if (Request.Cookies.TryGetValue(CookieNames.SpotifyAccessToken, out var token) 
                && _authenticationService.IsTokenValid(token))
            {
                return Ok(new { authenticated = true });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Get the Spotify login URL for OAuth flow
        /// </summary>
        /// <returns>Spotify authorization URL</returns>
        [HttpGet("login")]
        public string Login()
        {
            return _authenticationService.GenerateSpotifyLoginUrl();
        }

        /// <summary>
        /// Spotify OAuth callback endpoint
        /// Handles the authorization code and exchanges it for an access token
        /// </summary>
        /// <param name="code">Authorization code from Spotify</param>
        /// <param name="state">State parameter containing the PKCE verifier</param>
        /// <returns>Redirect to frontend</returns>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            try
            {
                var accessToken = await _authenticationService.ExchangeCodeForAccessToken(code, state);
                SetAccessTokenCookie(accessToken);

                var frontendUrl = _configuration[FrontendUrlConfigKey]!;
                return Redirect(frontendUrl);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Logout the user by deleting the authentication cookie
        /// </summary>
        /// <returns>OK response</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(CookieNames.SpotifyAccessToken);
            return Ok();
        }

        /// <summary>
        /// Debug endpoint: Manually set the access token cookie
        /// WARNING: For development/testing only, should not be available in production
        /// </summary>
        [HttpGet("debug/set-token")]
        [Conditional("DEBUG")]
        public void DebugSetAccessToken(string accessToken)
        {
            if (!_env.IsDevelopment())
                throw new InvalidOperationException("This endpoint is only available in development.");
            
            SetAccessTokenCookie(accessToken);
        }

        /// <summary>
        /// Helper method to set the access token cookie with secure options
        /// </summary>
        /// <param name="accessToken">The access token to store in the cookie</param>
        private void SetAccessTokenCookie(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.Add(TokenExpiration)
            };
            Response.Cookies.Append(CookieNames.SpotifyAccessToken, accessToken, cookieOptions);
        }
    }
}
