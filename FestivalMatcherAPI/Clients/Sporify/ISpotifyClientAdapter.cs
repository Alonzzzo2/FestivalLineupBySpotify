using FestivalMatcherAPI.Clients.Spotify.Models;

namespace FestivalMatcherAPI.Clients.Spotify
{
    public interface ISpotifyClientAdapter
    {
        string ClientId { get; }
        Uri RedirectUri { get; }
        
        Task<List<Artist>> GetArtistsFromLikedSongsAsync();
        
        Task<List<Artist>> GetArtistsFromPublicPlaylistAsync(string playlistUrl);
    }
}
