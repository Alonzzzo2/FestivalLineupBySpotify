namespace Spotify_Alonzzo_API.Clients.Spotify.Utilities
{
    /// <summary>
    /// Utility methods for Spotify playlist operations
    /// </summary>
    public static class SpotifyPlaylistUtility
    {
        /// <summary>
        /// Extracts and validates the Spotify playlist ID from a public playlist URL
        /// </summary>
        /// <param name="playlistUrl">Public Spotify playlist URL</param>
        /// <returns>The extracted playlist ID</returns>
        /// <exception cref="ArgumentException">Thrown if URL is invalid or not a Spotify playlist URL</exception>
        public static string ExtractPlaylistId(string playlistUrl)
        {
            if (string.IsNullOrWhiteSpace(playlistUrl))
                throw new ArgumentException("Playlist URL cannot be empty", nameof(playlistUrl));
            
            try
            {
                var uri = new Uri(playlistUrl);
                
                // Validate domain
                if (!uri.Host.Equals("open.spotify.com", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("URL must be from open.spotify.com");
                
                // Extract ID from path: /playlist/ID
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (segments.Length < 2 || !segments[0].Equals("playlist", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Invalid Spotify playlist URL format");
                
                var playlistId = segments[1];
                
                // Validate ID format
                if (string.IsNullOrWhiteSpace(playlistId) || playlistId.Length < 10)
                    throw new ArgumentException("Invalid playlist ID format");
                
                return playlistId;
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Invalid URL format", nameof(playlistUrl), ex);
            }
        }
    }
}
