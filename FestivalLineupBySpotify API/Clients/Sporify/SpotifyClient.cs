using FestivalLineupBySpotify_API.Configuration;
using FestivalLineupBySpotify_API.Constants;
using Microsoft.Extensions.Options;
using Spotify_Alonzzo_API.Clients.Spotify.Models;
using Spotify_Alonzzo_API.Clients.Spotify.Utilities;
using SpotifyAPI.Web;

namespace Spotify_Alonzzo_API.Clients.Spotify
{
    public class SpotifyClient : ISpotifyClient
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;

        public string ClientId => _clientId;
        public Uri RedirectUri => _redirectUri;

        public SpotifyClient(IOptions<SpotifySettings> options)
        {
            var settings = options.Value;
            _clientId = settings.ClientId ?? throw new ArgumentNullException(nameof(settings.ClientId));
            _clientSecret = settings.ClientSecret ?? throw new ArgumentNullException(nameof(settings.ClientSecret));
            _redirectUri = new Uri(settings.RedirectUri ?? throw new ArgumentNullException(nameof(settings.RedirectUri)));
        }

        public SpotifyAPI.Web.SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies)
        {
            if (!cookies.TryGetValue(CookieNames.SpotifyAccessToken, out var accessToken))
            {
                throw new Exception($"Missing cookie {CookieNames.SpotifyAccessToken}");
            }
            if (accessToken == null)
            {
                throw new Exception("Null access token!");
            }
            return new SpotifyAPI.Web.SpotifyClient(accessToken);
        }

        public async Task<List<Artist>> GetArtistsFromLikedSongsAsync(SpotifyAPI.Web.SpotifyClient spotifyClient)
        {
            var likedTracksPage = await spotifyClient.Library.GetTracks(new LibraryTracksRequest() { Limit = 50 });
            var likedTracks = await spotifyClient.PaginateAll(likedTracksPage);
            
            if (likedTracks == null || likedTracks.Count == 0)
            {
                throw new Exception("Couldn't find any liked tracks");
            }

            var likedArtists = likedTracks
                .SelectMany(track => track.Track.Artists)
                .Select(artist => artist.Name)
                .ToList();

            return ExtractArtistCountsFromNames(likedArtists);
        }

        /// <summary>
        /// Extracts artist names from a collection of tracks and counts occurrences
        /// </summary>
        /// <remarks>
        /// This method is reused by both liked tracks and public playlist methods
        /// to maintain consistent artist aggregation logic.
        /// </remarks>
        private static List<Artist> ExtractArtistCountsFromNames(List<string> artistNames)
        {
            return artistNames
                .GroupBy(artist => artist)
                .Select(g => new Artist(g.Key, g.Count()))
                .ToList();
        }

        public async Task<List<Artist>> GetArtistsFromPublicPlaylistAsync(string playlistUrl)
        {
            // Extract and validate playlist ID using shared utility
            var playlistId = SpotifyPlaylistUtility.ExtractPlaylistId(playlistUrl);
            
            // Create client for public API using client credentials
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new ClientCredentialsAuthenticator(_clientId, _clientSecret));
            var spotifyClient = new SpotifyAPI.Web.SpotifyClient(config);
            
            try
            {
                // Get playlist tracks (paginated)
                var playlistTracks = await spotifyClient.Playlists.GetItems(playlistId);
                var allTracks = await spotifyClient.PaginateAll(playlistTracks);
                
                if (allTracks == null || allTracks.Count == 0)
                {
                    throw new InvalidOperationException("Couldn't find any tracks in playlist");
                }
                
                // Extract artist names from tracks
                var artistNames = allTracks
                    .Where(t => t.Track is FullTrack)
                    .SelectMany(t => ((FullTrack)t.Track).Artists)
                    .Select(artist => artist.Name)
                    .ToList();
                
                // Group and count artists using shared logic
                return ExtractArtistCountsFromNames(artistNames);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Playlist not found", ex);
            }
        }
    }
}
