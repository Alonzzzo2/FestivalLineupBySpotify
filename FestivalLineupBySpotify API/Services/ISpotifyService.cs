using FestivalLineupBySpotify_API.Models;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<List<ArtistInfo>> GetFavoriteArtists(bool forceReloadData = false);
    }
}