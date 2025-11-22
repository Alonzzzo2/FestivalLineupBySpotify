using FestivalLineupBySpotify_API.Models;
using Spotify_Alonzzo_API.Clients.ClashFinders;
using Spotify_Alonzzo_API.Clients.ClashFinders.Models;
using Spotify_Alonzzo_API.Clients.Sporify.Models;

namespace FestivalLineupBySpotify_API.Services
{
    public class ClashFindersService : IClashFindersService
    {
        private readonly ClashFindersClient _clashFindersClient;

        public ClashFindersService(ClashFindersClient clashFindersClient)
        {
            _clashFindersClient = clashFindersClient;
        }

        public async Task<List<FestivalListItemModel>> GetAllFestivals()
        {
            var festivals = await _clashFindersClient.GetAllFestivals();
            return festivals.Select(f => new FestivalListItemModel
            {
                Title = f.Title,
                InternalName = f.InternalName,
                StartDate = f.StartDate,
                PrintAdvisory = f.PrintAdvisory
            }).ToList();
        }

        public async Task<Festival> GetFestival(string internalFestivalName)
        {
            return await _clashFindersClient.GetFestival(internalFestivalName);
        }

        public async Task<List<Festival>> GetFestivalsByYear(int year)
        {
            return await _clashFindersClient.GetAllFestivalsByYear(year);
        }

        public string BuildHighlightUrl(string festivalId, List<string>[] artistsByPriority)
        {
            if (string.IsNullOrEmpty(festivalId))
                throw new ArgumentNullException(nameof(festivalId), "Festival ID cannot be null or empty");

            if (artistsByPriority == null || artistsByPriority.Length == 0)
                throw new ArgumentException("At least one artist priority tier must be provided", nameof(artistsByPriority));

            var highlightsCollection = new HighlightsCollection();

            // Populate highlights with artists organized by priority
            for (int i = 0; i < artistsByPriority.Length && i < 4; i++)
            {
                if (artistsByPriority[i]?.Count > 0)
                {
                    highlightsCollection[i].ArtistsShortEventNames.AddRange(artistsByPriority[i]);
                }
            }

            return highlightsCollection.GenerateUrl(festivalId);
        }
    }
}
