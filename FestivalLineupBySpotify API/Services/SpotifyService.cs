using Newtonsoft.Json;
using Spotify_Alonzzo_API.Clients.Sporify;
using Spotify_Alonzzo_API.Clients.Sporify.Models;
using Spotify_Alonzzo_API.Services;
using System.Text;

namespace FestivalLineupBySpotify_API.Services
{
    public class SpotifyService : ISpotifyService
    {
        private const string FavoriteArtistsCacheKey = "favorite_artists";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISpotifyClient _spotifyClient;

        public SpotifyService(IHttpContextAccessor httpContextAccessor, ISpotifyClient spotifyApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _spotifyClient = spotifyApiService;
        }

        public async Task<List<Artist>> GetFavoriteArtists(bool forceReloadData = false)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available.");
            }

            // Try to get from cache first
            var cachedArtists = GetCachedFavoriteArtists(httpContext);
            if (!forceReloadData && cachedArtists != null)
            {
                return cachedArtists;
            }

            return await GetFavoriteArtistsFromSpotify(httpContext);
        }

        private List<Artist>? GetCachedFavoriteArtists(HttpContext httpContext)
        {
            if (!httpContext.Session.TryGetValue(FavoriteArtistsCacheKey, out byte[]? dataBytes))
            {
                return null;
            }

            var data = Encoding.UTF8.GetString(dataBytes);
            return JsonConvert.DeserializeObject<List<Artist>>(data) ?? new List<Artist>();
        }

        private async Task<List<Artist>> GetFavoriteArtistsFromSpotify(HttpContext httpContext)
        {
            var cookies = httpContext.Request.Cookies 
                ?? throw new InvalidOperationException("No HTTP context available. Cannot access request cookies.");
            var spotifyClient = _spotifyClient.CreateSpotifyClient(cookies);
            var favoriteArtists = await _spotifyClient.GetFavoriteArtistsFromSpotify(spotifyClient);
            
            CacheFavoriteArtists(httpContext, favoriteArtists);
            return favoriteArtists;
        }

        private void CacheFavoriteArtists(HttpContext httpContext, List<Artist> artists)
        {
            var serializedArtists = JsonConvert.SerializeObject(artists);
            httpContext.Session.SetString(FavoriteArtistsCacheKey, serializedArtists);
        }
    }
}
