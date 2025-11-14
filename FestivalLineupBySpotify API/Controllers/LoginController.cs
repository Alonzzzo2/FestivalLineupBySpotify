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
            // Store verifier in session instead of cookies to support cross-domain scenarios
            HttpContext.Session.SetString("SpotifyCodeVerifier", generatedCode.verifier);
            var challenge = generatedCode.challenge;
            var loginRequest = new LoginRequest(
              _spotifyApiService.RedirectUri,
              _spotifyApiService.ClientId,
              LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserLibraryRead,  }
            };
            var uri = loginRequest.ToUri();
            //The client should call this, after a successful login, the Callback endpoint will be called in order to create an access token
            return uri.ToString(); 
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            HttpContext.Session.Remove("SpotifyCodeVerifier");
            return Ok();
        }


        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Callback(string code)
        {
            // Retrieve verifier from session
            var verifier = HttpContext.Session.GetString("SpotifyCodeVerifier");
            
            if (string.IsNullOrEmpty(verifier))
            {
                return BadRequest("PKCE verifier not found in session. Please start authentication again.");
            }

            var initialResponse = await new OAuthClient().RequestToken(
              new PKCETokenRequest(_spotifyApiService.ClientId, code, _spotifyApiService.RedirectUri, verifier)
            );

            Response.Cookies.Append("AccessToken", initialResponse.AccessToken);
            
            // Clear the verifier after use
            HttpContext.Session.Remove("SpotifyCodeVerifier");
            
            var frontendUrl = _configuration["FrontendUrl"]!;
            
            return Redirect(frontendUrl);
        }

        [Route("[action]")]
        [HttpGet]
        public string DebugSetAccessToken(string accessToken)
        {
            Response.Cookies.Append("AccessToken", accessToken);
            return accessToken;            
        }

    }
}
