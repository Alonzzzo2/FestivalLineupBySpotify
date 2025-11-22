using Spotify_Alonzzo_API.Clients.Sporify.Models;

namespace Spotify_Alonzzo_API.Clients.Sporify
{
    public interface ISpotifyClient
    {
        string ClientId { get; }
        Uri RedirectUri { get; }
        SpotifyAPI.Web.SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies);
        Task<List<Artist>> GetFavoriteArtistsFromSpotify(SpotifyAPI.Web.SpotifyClient spotifyClient);
    }
}
