using Spotify_Alonzzo_API.Clients.Models;

namespace Spotify_Alonzzo_API.Clients
{
    public interface ISpotifyClient
    {
        string ClientId { get; }
        Uri RedirectUri { get; }
        SpotifyAPI.Web.SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies);
        Task<List<Artist>> GetFavoriteArtistsFromSpotify(SpotifyAPI.Web.SpotifyClient spotifyClient);
    }
}
