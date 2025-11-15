using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using FestivalLineupBySpotify_API.Services;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISpotifyApiService _spotifyApiService;

        public LoginController(IConfiguration configuration, ISpotifyApiService spotifyApiService)
        {
            _configuration = configuration;
            _spotifyApiService = spotifyApiService;
        }

        [Route("[action]")]
        [HttpGet]
        public string Login()
        {
            var generatedCode = PKCEUtil.GenerateCodes();            
            var challenge = generatedCode.challenge;
            
            // Encode verifier in state parameter (will be returned by Spotify)
            var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(generatedCode.verifier));
            
            var loginRequest = new LoginRequest(
              _spotifyApiService.RedirectUri,
              _spotifyApiService.ClientId,
              LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserLibraryRead,  },
                State = state
            };
            var uri = loginRequest.ToUri();
            //The client should call this, after a successful login, the Callback endpoint will be called in order to create an access token
            return uri.ToString(); 
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            return Ok();
        }


        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Callback(string code, string state)
        {
            // Decode verifier from state parameter
            if (string.IsNullOrEmpty(state))
            {
                return BadRequest("State parameter missing. Please start authentication again.");
            }

            string verifier;
            try
            {
                verifier = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
            }
            catch
            {
                return BadRequest("Invalid state parameter. Please start authentication again.");
            }

            var initialResponse = await new OAuthClient().RequestToken(
              new PKCETokenRequest(_spotifyApiService.ClientId, code, _spotifyApiService.RedirectUri, verifier)
            );

            // Set cookie with appropriate SameSite policy for cross-origin requests
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS only in production
                SameSite = SameSiteMode.None, // Allow cross-origin (requires Secure=true)
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("AccessToken", initialResponse.AccessToken, cookieOptions);
            
            var frontendUrl = _configuration["FrontendUrl"]!;
            
            return Redirect(frontendUrl);
        }

        [Route("[action]")]
        [HttpGet]
        public string DebugSetAccessToken(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            return accessToken;            
        }

    }
}
