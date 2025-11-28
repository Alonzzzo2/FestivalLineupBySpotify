using FestivalMatcherAPI.Clients.Spotify.Models;
using FestivalMatcherAPI.Clients.Spotify.Utilities;
using FestivalMatcherAPI.Configuration;
using FestivalMatcherAPI.Constants;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;

namespace FestivalMatcherAPI.Clients.Spotify
{
    public class SpotifyClientAdapter : ISpotifyClientAdapter
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string ClientId => _clientId;
        public Uri RedirectUri => _redirectUri;

        public SpotifyClientAdapter(IOptions<SpotifySettings> options, IHttpContextAccessor httpContextAccessor)
        {
            var settings = options.Value;
            _clientId = settings.ClientId ?? throw new ArgumentNullException(nameof(settings.ClientId));
            _clientSecret = settings.ClientSecret ?? throw new ArgumentNullException(nameof(settings.ClientSecret));
            _redirectUri = new Uri(settings.RedirectUri ?? throw new ArgumentNullException(nameof(settings.RedirectUri)));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Artist>> GetArtistsFromLikedSongsAsync()
        {
            var spotifyClient = CreateAuthenticatedClient();

            var likedTracksPage = await spotifyClient.Library.GetTracks(new LibraryTracksRequest() { Limit = 50 });
            var likedTracks = await spotifyClient.PaginateAll(likedTracksPage);
            return await GetArtistsWeightFromTracks(spotifyClient, likedTracks.Select(t => t.Track));
        }

        public async Task<List<Artist>> GetArtistsFromPublicPlaylistAsync(string playlistUrl)
        {
            var spotifyClient = CreateDefaultClient();
            var playlistId = SpotifyPlaylistUtility.ExtractPlaylistId(playlistUrl);

            try
            {
                var playlistTracksPahe = await spotifyClient.Playlists.GetItems(playlistId);
                var playlistTracks = await spotifyClient.PaginateAll(playlistTracksPahe);
                return await GetArtistsWeightFromTracks(spotifyClient, playlistTracks.Select(t => (FullTrack)t.Track));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Playlist not found", ex);
            }
        }

        private SpotifyClient CreateDefaultClient()
        {
            var config = SpotifyClientConfig
                            .CreateDefault()
                            .WithAuthenticator(new ClientCredentialsAuthenticator(_clientId, _clientSecret));
            var spotifyClient = new SpotifyClient(config);
            return spotifyClient;
        }

        private SpotifyClient CreateAuthenticatedClient()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HTTP context available.");

            if (!httpContext.Request.Cookies.TryGetValue(CookieNames.SpotifyAccessToken, out var accessToken))
            {
                throw new Exception($"Missing cookie {CookieNames.SpotifyAccessToken}");
            }
            if (accessToken == null)
            {
                throw new Exception("Null access token!");
            }
            return new SpotifyClient(accessToken);
        }        

        private static async Task<List<Artist>> GetArtistsWeightFromTracks(SpotifyClient spotifyClient, IEnumerable<FullTrack> tracks)
        {
            if (tracks == null || !tracks.Any())
            {
                throw new Exception("Couldn't find any tracks");
            }

            var artistsWeights = tracks
                .SelectMany(track => track.Artists)
                .GroupBy(artist => artist.Name)
                .Select(g => new Artist(g.Key, g.Count()))
                .ToList();

            return artistsWeights;
        }
    }
}
