using FestivalLineupBySpotify_API.Configuration;
using FestivalLineupBySpotify_API.DTO;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;

namespace FestivalLineupBySpotify_API.Services
{
    public interface ISpotifyApiService
    {
        string ClientId { get; }
        Uri RedirectUri { get; }
        SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies);
        Task<List<Artist>> GetFavoriteArtistsFromSpotify(SpotifyClient spotifyClient);
    }

    public class SpotifyApiService : ISpotifyApiService
    {
        private const string AccessTokenCookeName = "AccessToken";
        private readonly string _clientId;
        private readonly Uri _redirectUri;

        public string ClientId => _clientId;
        public Uri RedirectUri => _redirectUri;

        public SpotifyApiService(IOptions<SpotifySettings> options)
        {
            var settings = options.Value;
            _clientId = settings.ClientId ?? throw new ArgumentNullException(nameof(settings.ClientId));
            _redirectUri = new Uri(settings.RedirectUri ?? throw new ArgumentNullException(nameof(settings.RedirectUri)));
        }

        public SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies)
        {
            if (!cookies.TryGetValue(AccessTokenCookeName, out var accessToken))
            {
                throw new Exception($"Missing cookie {AccessTokenCookeName}");
            }
            if (accessToken == null)
            {
                throw new Exception("Null access token!");
            }
            return new SpotifyClient(accessToken);
        }

        public async Task<List<Artist>> GetFavoriteArtistsFromSpotify(SpotifyClient spotifyClient)
        {
            var likedTracksPage = await spotifyClient.Library.GetTracks(new LibraryTracksRequest() { Limit = 50 });
            var likedTracks = await spotifyClient.PaginateAll(likedTracksPage);
            
            if (likedTracks == null || likedTracks.Count == 0)
            {
                throw new Exception("Couldn't find any liked tracks");
            }

            var likedArtists = likedTracks.SelectMany(track => track.Track.Artists).Select(artist => artist.Name).ToList();
            var favArtists = likedArtists.GroupBy(artist => artist).Select(g => new Artist(g.Key, g.Count())).ToList();
            return favArtists;
        }
    }
}
