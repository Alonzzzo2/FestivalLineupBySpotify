using FestivalMatcherAPI.Clients.Spotify;
using FestivalMatcherAPI.Clients.Spotify.Utilities;
using FestivalMatcherAPI.Configuration;
using FestivalMatcherAPI.Models;
using FestivalMatcherAPI.Services.Caching;
using Microsoft.Extensions.Options;

namespace FestivalMatcherAPI.Services
{
    public class SpotifyService : ISpotifyService
    {
        // Cache key prefixes for different scenarios
        private const string LikedSongsCacheKeyPrefix = "liked_songs_";
        private const string PlaylistCacheKeyPrefix = "playlist_";
        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISpotifyClientAdapter _spotifyClient;
        private readonly ICacheService _cacheService;
        private readonly IOptions<CacheSettings> _cacheSettings;

        public SpotifyService(
            IHttpContextAccessor httpContextAccessor, 
            ISpotifyClientAdapter spotifyApiService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _spotifyClient = spotifyApiService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings;
        }

        public async Task<List<ArtistInfo>> GetArtistsFromLikedSongs()
        {
            var httpContext = GetHttpContext();
            var cookieId = httpContext.Session.Id;
            var cacheKey = GenerateCacheKey(LikedSongsCacheKeyPrefix, cookieId);
            var ttl = TimeSpan.FromMinutes(_cacheSettings.Value.LikedSongsTtlMinutes);

            return await _cacheService.GetOrFetchAndSetAsync(cacheKey, () => FetchArtistsFromLikedSongs(httpContext), new () { SlidingExpiration = ttl });
        }

        public async Task<List<ArtistInfo>> GetArtistsFromPublicPlaylist(string playlistUrl)
        {
            var playlistId = SpotifyPlaylistUtility.ExtractPlaylistId(playlistUrl);
            var cacheKey = GenerateCacheKey(PlaylistCacheKeyPrefix, playlistId);
            var ttl = TimeSpan.FromMinutes(_cacheSettings.Value.PlaylistTtlMinutes);

            return await _cacheService.GetOrFetchAndSetAsync(cacheKey, () => FetchArtistsFromPublicPlaylist(playlistUrl), new() { SlidingExpiration = ttl });
        }

        private async Task<List<ArtistInfo>> FetchArtistsFromLikedSongs(HttpContext httpContext)
        {
            var artistsFromLikedSongs = await _spotifyClient.GetArtistsFromLikedSongsAsync();            
            return [.. artistsFromLikedSongs.Select(a => new ArtistInfo(a.Name, a.NumOfLikedTracks))];
        }

        private async Task<List<ArtistInfo>> FetchArtistsFromPublicPlaylist(string playlistUrl)
        {
            var artists = await _spotifyClient.GetArtistsFromPublicPlaylistAsync(playlistUrl);            
            return [.. artists.Select(a => new ArtistInfo(a.Name, a.NumOfLikedTracks))];
        }

        private static string GenerateCacheKey(string prefix, string identifier)
        {
            return $"{prefix}{identifier}";
        }

        private HttpContext GetHttpContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available.");
            }
            return httpContext;
        }
    }
}
