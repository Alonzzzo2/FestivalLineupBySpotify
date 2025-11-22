using FestivalLineupBySpotify_API.Models;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<List<ArtistInfo>> GetArtistsFromLikedSongs();
        
        /// <summary>
        /// Get artists from a public Spotify playlist without authentication
        /// </summary>
        /// <param name="playlistUrl">Public playlist URL</param>
        /// <returns>List of artists from the playlist</returns>
        Task<List<ArtistInfo>> GetArtistsFromPublicPlaylist(string playlistUrl);
    }
}