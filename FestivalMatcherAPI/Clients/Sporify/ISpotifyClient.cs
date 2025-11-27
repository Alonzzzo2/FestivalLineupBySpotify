using FestivalMatcherAPI.Clients.Spotify.Models;

namespace FestivalMatcherAPI.Clients.Spotify
{
    public interface ISpotifyClient
    {
        string ClientId { get; }
        Uri RedirectUri { get; }
        SpotifyAPI.Web.SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies);
        
        /// <summary>
        /// Get artists from user's liked tracks (requires authentication)
        /// </summary>
        /// <param name="spotifyClient">Authenticated Spotify client</param>
        /// <returns>List of artists from liked tracks</returns>
        Task<List<Artist>> GetArtistsFromLikedSongsAsync(SpotifyAPI.Web.SpotifyClient spotifyClient);
        
        /// <summary>
        /// Get artists from a public Spotify playlist without authentication
        /// </summary>
        /// <param name="playlistUrl">Public playlist URL</param>
        /// <returns>List of artists in the playlist</returns>
        Task<List<Artist>> GetArtistsFromPublicPlaylistAsync(string playlistUrl);
    }
}
