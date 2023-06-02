using FestivalLineupBySpotify_API.DTO;
using SpotifyAPI.Web;

namespace FestivalLineupBySpotify_API.Services
{
    public static class SpotifyApiService
    {
        private const string accessTokenCookeName = "AccessToken";

        public static string clientId = Environment.GetEnvironmentVariable("CLIENT_ID");


        public static SpotifyClient CreateSpotifyClient(IRequestCookieCollection cookies)
        {
            if (!cookies.TryGetValue(accessTokenCookeName, out var accessToken))
            {
                throw new Exception($"Missing cookie {accessTokenCookeName}");
            }
            if (accessToken== null)
            {
                throw new Exception($"Null access token!");
            }
            var spotify = new SpotifyClient(accessToken);
            return spotify;
        }

        public static async Task<List<Artist>> GetFavoriteArtistsFromSpotify(SpotifyClient spotifyClient)
        {
            var likedTracksPage = await spotifyClient.Library.GetTracks(new LibraryTracksRequest() { Limit = 50 });
            // var libraryTracks = libraryTracksPage.Items;
            var likedTracks = await spotifyClient.PaginateAll(likedTracksPage);
            if (likedTracks == null || likedTracks.Count == 0)
            {
                throw new Exception("Couldn't find any liked tracks");
            }

            var likedArtists = likedTracks.SelectMany(track => track.Track.Artists).Select(artist => artist.Name).ToList();
            var favArtists = likedArtists.GroupBy(artist => artist).Select(g => new Artist(g.Key, g.Count())).ToList();
            return favArtists;
            // todo - store this data for mock for tests
        }
    }
}
