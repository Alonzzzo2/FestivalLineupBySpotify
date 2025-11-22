using Spotify_Alonzzo_API.Clients.Sporify.Models;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<List<Artist>> GetFavoriteArtists(bool forceReloadData = false);
    }
}