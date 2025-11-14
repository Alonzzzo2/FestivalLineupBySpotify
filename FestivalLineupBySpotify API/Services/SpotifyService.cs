using Newtonsoft.Json;
using Spotify_Alonzzo_API.Services;
using SpotifyAPI.Web.Http;
using System.Text;
using System.Text.RegularExpressions;
using FestivalLineupBySpotify_API.Services;

namespace FestivalLineupBySpotify_API.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClashFindersService _clashFindersService;
        private readonly ISpotifyApiService _spotifyApiService;

        public SpotifyService(IHttpContextAccessor httpContextAccessor, ClashFindersService clashFindersService, ISpotifyApiService spotifyApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _clashFindersService = clashFindersService;
            _spotifyApiService = spotifyApiService;
        }

        public async Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear)
        {
            var festivalEvents = await _clashFindersService.GetAllEventsByYear(festivalsYear);
            List<ClashFindersFavoritesResult> results = new List<ClashFindersFavoritesResult>();
            festivalEvents.ForEach(festival => {
                var result = this.GenerateClashFindersFavoritesResult(request, festival.Name).Result;
                result.FestivalEvent = festival;
                results.Add(result);
            });
            return results.OrderByDescending(r => r.Rank).ToList();
        }

        public async Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName, bool forceReloadData = false)
        {
            var favArtists = await GetFavArtists(request, forceReloadData);
            var festivalEvents = await _clashFindersService.GetEventsFromClashFinders(festivalName);

            var artistsWithEvents = GenerateArtistsWithEvents(favArtists, festivalEvents);
            var highlightsCollection = new ClashFindersService.HighlightsCollection();

            double groupSize = (double)artistsWithEvents.Count / (double)4;
            for (int i = 0; i < artistsWithEvents.Count; i++)
            {
                int normalizedScore = (int)(Math.Floor((double)(i / groupSize)));                
                var clashFindersShortArtistsNames = artistsWithEvents[i].Events
                    .Select(e => e.Short.Split('(')[0])
                    .Distinct()
                    .ToList();
                highlightsCollection[normalizedScore].ArtistsShortEventNames.AddRange(clashFindersShortArtistsNames);                                
            }

            var result = new ClashFindersFavoritesResult(highlightsCollection.GenerateUrl(festivalName), artistsWithEvents.Sum(a => a.NumOfLikedTracks));
            return result;
        }

        private async Task<List<DTO.Artist>> GetFavArtists(HttpRequest request, bool forceReloadData)
        {
            if (forceReloadData || !_httpContextAccessor.HttpContext.Session.TryGetValue("data", out byte[] dataBytes))
            {
                return await GetFavArtistsFromSpotify(request);
            }
            var data = Encoding.UTF8.GetString(dataBytes);
            return JsonConvert.DeserializeObject<List<DTO.Artist>>(data);
        }

        private async Task<List<DTO.Artist>> GetFavArtistsFromSpotify(HttpRequest request)
        {
            var spotifyClient = _spotifyApiService.CreateSpotifyClient(request.Cookies);
            var favArtists = await _spotifyApiService.GetFavoriteArtistsFromSpotify(spotifyClient);
            _httpContextAccessor.HttpContext.Session.SetString("data", JsonConvert.SerializeObject(favArtists));
            return favArtists;
        }

        private static List<DTO.Artist> GenerateArtistsWithEvents(List<DTO.Artist> favArtists, List<DTO.Event> festivalEvents)
        {
            // for each fav artist, if he has a festival event, add that event to the artist's event
            favArtists.ForEach(favArtist =>
            {
                var favArtistNames = favArtist.Name.ToLower().Split(" ");
                var favArtistEvents = festivalEvents.Where(festivalEvent =>
                {
                    char[] separators = { ';', ',', '-', ' ' };
                    var festivalEventArtists = festivalEvent.Name.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    bool isFavArtistEvent = favArtistNames.All(favArtistName => festivalEventArtists.Contains(favArtistName));
                    return isFavArtistEvent;    
                }).ToList();
                
                favArtist.Events.AddRange(favArtistEvents);
            });
            return favArtists.Where(favArtist => favArtist.Events.Any()).OrderByDescending(a => a.NumOfLikedTracks).ToList();
        }
    }
}
