using FestivalLineupBySpotify_API.DTO;
using FestivalLineupBySpotify_API.Models;
using Newtonsoft.Json;
using Spotify_Alonzzo_API.Clients;
using Spotify_Alonzzo_API.Clients.Models;
using Spotify_Alonzzo_API.Services;
using System.Text;

namespace FestivalLineupBySpotify_API.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClashFindersClient _clashFindersClient;
        private readonly ISpotifyClient _spotifyClient;

        public SpotifyService(IHttpContextAccessor httpContextAccessor, ClashFindersClient clashFindersService, ISpotifyClient spotifyApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _clashFindersClient = clashFindersService;
            _spotifyClient = spotifyApiService;
        }

        public async Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear)
        {
            var festivals = await _clashFindersClient.GetAllFestivalsByYear(festivalsYear);
            List<ClashFindersFavoritesResult> results = new List<ClashFindersFavoritesResult>();
            festivals.ForEach(festival => {
                var result = this.GenerateClashFindersFavoritesResult(request, festival).Result;                
                results.Add(result);
            });
            return results.OrderByDescending(r => r.Rank).ToList();
        }

        public async Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string internalFestivalName, bool forceReloadData = false)
        {
            var festival = await _clashFindersClient.GetFestival(internalFestivalName);
            return await GenerateClashFindersFavoritesResult(request, festival, forceReloadData);
        }

        public async Task<List<FestivalListItemResponse>> GetAllFestivals()
        {
            var festivals = await _clashFindersClient.GetAllFestivals();
            return festivals.Select(f => new FestivalListItemResponse
            {
                Title = f.Title,
                InternalName = f.InternalName,
                StartDate = f.StartDate,
                PrintAdvisory = f.PrintAdvisory
            }).ToList();
        }

        public async Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, Festival festival, bool forceReloadData = false)
        {
            var favArtists = await GetFavArtists(request, forceReloadData);

            var artistsWithEvents = GenerateArtistsWithEvents(favArtists, festival.Locations.SelectMany(location => location.Events).ToList());
            var highlightsCollection = new ClashFindersClient.HighlightsCollection();

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

            var result = new ClashFindersFavoritesResult(highlightsCollection.GenerateUrl(festival.Id), artistsWithEvents.Sum(a => a.NumOfLikedTracks), festival);
            return result;
        }

        private async Task<List<Artist>> GetFavArtists(HttpRequest request, bool forceReloadData)
        {
            if (forceReloadData || !_httpContextAccessor.HttpContext.Session.TryGetValue("data", out byte[] dataBytes))
            {
                return await GetFavArtistsFromSpotify(request);
            }
            var data = Encoding.UTF8.GetString(dataBytes);
            return JsonConvert.DeserializeObject<List<Artist>>(data);
        }

        private async Task<List<Artist>> GetFavArtistsFromSpotify(HttpRequest request)
        {
            var spotifyClient = _spotifyClient.CreateSpotifyClient(request.Cookies);
            var favArtists = await _spotifyClient.GetFavoriteArtistsFromSpotify(spotifyClient);
            _httpContextAccessor.HttpContext.Session.SetString("data", JsonConvert.SerializeObject(favArtists));
            return favArtists;
        }

        private static List<Artist> GenerateArtistsWithEvents(List<Artist> favArtists, List<Event> festivalEvents)
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
