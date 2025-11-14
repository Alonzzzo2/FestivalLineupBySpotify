using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;

namespace FestivalLineupBySpotify_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [Route("[action]")]
        [HttpGet]
        public string Login()
        {
            var generatedCode = PKCEUtil.GenerateCodes();            
            Response.Cookies.Append("Verifier", generatedCode.verifier);
            var challenge = generatedCode.challenge;
            var loginRequest = new LoginRequest(
              Services.SpotifyApiService.redirectUri,
              Services.SpotifyApiService.clientId,
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
            return Ok();
        }


        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Callback(string code)
        {
            var verifier = Request.Cookies["Verifier"];
            var initialResponse = await new OAuthClient().RequestToken(
              new PKCETokenRequest(Services.SpotifyApiService.clientId, code, Services.SpotifyApiService.redirectUri, verifier)
            );

            Response.Cookies.Append("AccessToken", initialResponse.AccessToken);
            //return initialResponse.AccessToken;
            // Todo - Also important for later: response.RefreshToken
            return Redirect("http://localhost:5173/");
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
