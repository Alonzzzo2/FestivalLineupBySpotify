using Newtonsoft.Json;
using FestivalLineupBySpotify_API.Models;
using FestivalLineupBySpotify_API.Services.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Spotify_Alonzzo_API.Clients.Spotify;
using Spotify_Alonzzo_API.Clients.Spotify.Utilities;
using Spotify_Alonzzo_API.Services;
using System.Text;

namespace FestivalLineupBySpotify_API.Services
{
    public class SpotifyService : ISpotifyService
    {
        private const string ArtistsFromLikedSongsCacheKey = "liked_songs_artists";
        private const string PublicPlaylistCacheKeyPrefix = "playlist_artists_";
        private static readonly TimeSpan PublicPlaylistCacheTtl = TimeSpan.FromHours(1);
        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISpotifyClient _spotifyClient;
        private readonly ICacheService _cacheService;

        public SpotifyService(
            IHttpContextAccessor httpContextAccessor, 
            ISpotifyClient spotifyApiService,
            ICacheService cacheService)
        {
            _httpContextAccessor = httpContextAccessor;
            _spotifyClient = spotifyApiService;
            _cacheService = cacheService;
        }

        public async Task<List<ArtistInfo>> GetArtistsFromLikedSongs()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available.");
            }

            // Try to get from cache first
            var cachedArtists = GetCachedArtistsFromLikedSongs(httpContext);
            if (cachedArtists != null)
            {
                return cachedArtists;
            }

            return await GetArtistsFromLikedSongs(httpContext);
        }

        public async Task<List<ArtistInfo>> GetArtistsFromPublicPlaylist(string playlistUrl)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(playlistUrl))
            {
                throw new ArgumentException("Playlist URL cannot be empty", nameof(playlistUrl));
            }
            
            // Generate cache key from URL using shared utility
            var cacheKey = GeneratePlaylistCacheKey(playlistUrl);
            
            // Try to get from cache first
            var cachedArtists = await _cacheService.GetAsync<List<ArtistInfo>>(cacheKey);
            if (cachedArtists != null)
            {
                return cachedArtists;
            }
            
            // Not in cache, fetch from Spotify
            var artists = await GetArtistsFromPlaylistSpotify(playlistUrl);
            
            // Cache with 24-hour TTL
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PublicPlaylistCacheTtl
            };
            
            await _cacheService.SetAsync(cacheKey, artists, cacheOptions);
            
            return artists;
        }

        private List<ArtistInfo>? GetCachedArtistsFromLikedSongs(HttpContext httpContext)
        {
            if (!httpContext.Session.TryGetValue(ArtistsFromLikedSongsCacheKey, out byte[]? dataBytes))
            {
                return null;
            }

            var data = Encoding.UTF8.GetString(dataBytes);
            return JsonConvert.DeserializeObject<List<ArtistInfo>>(data) ?? new List<ArtistInfo>();
        }

        private async Task<List<ArtistInfo>> GetArtistsFromLikedSongs(HttpContext httpContext)
        {
            var cookies = httpContext.Request.Cookies 
                ?? throw new InvalidOperationException("No HTTP context available. Cannot access request cookies.");
            var spotifyClient = _spotifyClient.CreateSpotifyClient(cookies);
            var artistsFromLikedSongs = await _spotifyClient.GetArtistsFromLikedSongsAsync(spotifyClient);
            
            // Convert client models to domain models
            var artistInfoList = artistsFromLikedSongs
                .Select(a => new ArtistInfo(a.Name, a.NumOfLikedTracks))
                .ToList();
            
            CacheArtistsFromLikedSongs(httpContext, artistInfoList);
            return artistInfoList;
        }

        private void CacheArtistsFromLikedSongs(HttpContext httpContext, List<ArtistInfo> artists)
        {
            var serializedArtists = JsonConvert.SerializeObject(artists);
            httpContext.Session.SetString(ArtistsFromLikedSongsCacheKey, serializedArtists);
        }

        /// <summary>
        /// Generates a cache key from a playlist URL by extracting the unique playlist ID
        /// </summary>
        private static string GeneratePlaylistCacheKey(string playlistUrl)
        {
            // Extract playlist ID using shared utility
            var playlistId = SpotifyPlaylistUtility.ExtractPlaylistId(playlistUrl);
            return $"{PublicPlaylistCacheKeyPrefix}{playlistId}";
        }

        private async Task<List<ArtistInfo>> GetArtistsFromPlaylistSpotify(string playlistUrl)
        {
            try
            {
                // Call client to fetch artists from Spotify public API
                var artists = await _spotifyClient.GetArtistsFromPublicPlaylistAsync(playlistUrl);
                
                // Convert to domain model (same conversion as authenticated flow)
                var artistInfoList = artists
                    .Select(a => new ArtistInfo(a.Name, a.NumOfLikedTracks))
                    .ToList();
                
                return artistInfoList;
            }
            catch (ArgumentException ex)
            {
                // URL validation failed in client layer
                throw new InvalidOperationException($"Invalid playlist URL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Spotify API error or other issue
                throw new InvalidOperationException(
                    "Failed to retrieve artists from playlist. Please check the URL and try again.", 
                    ex);
            }
        }
    }
}
