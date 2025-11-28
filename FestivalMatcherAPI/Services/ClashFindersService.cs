using FestivalMatcherAPI.Models;
using FestivalMatcherAPI.Services.Caching;
using Microsoft.Extensions.Caching.Distributed;
using FestivalMatcherAPI.Clients.ClashFinders;
using FestivalMatcherAPI.Clients.ClashFinders.Models;
using FestivalMatcherAPI.Clients.Spotify.Models;

namespace FestivalMatcherAPI.Services
{
    public class ClashFindersService : IClashFindersService
    {
        private readonly ClashFindersClient _clashFindersClient;
        private readonly ICacheService _cacheService;

        private const string AllFestivalsCacheKey = "cf:all_festivals";
        private const string FestivalCacheKeyPrefix = "cf:festival:";
        private const string FestivalsByYearCacheKeyPrefix = "cf:festivals_by_year:";

        private static readonly DistributedCacheEntryOptions FestivalListCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        private static readonly DistributedCacheEntryOptions FestivalDetailsCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public ClashFindersService(ClashFindersClient clashFindersClient, ICacheService cacheService)
        {
            _clashFindersClient = clashFindersClient;
            _cacheService = cacheService;
        }

        public async Task<List<FestivalListItemModel>> GetAllFestivals()
        {
            var cachedFestivals = await _cacheService.GetAsync<List<FestivalListItemModel>>(AllFestivalsCacheKey);
            if (cachedFestivals != null)
            {
                return cachedFestivals;
            }

            var festivals = await _clashFindersClient.GetAllFestivals();
            var festivalModels = festivals.Select(f => new FestivalListItemModel
            {
                Title = f.Title,
                InternalName = f.InternalName,
                StartDate = f.StartDate,
                PrintAdvisory = (int)f.PrintAdvisory
            }).ToList();

            await _cacheService.SetAsync(AllFestivalsCacheKey, festivalModels, FestivalListCacheOptions);
            return festivalModels;
        }

        public async Task<FestivalData> GetFestival(string internalFestivalName)
        {
            var cacheKey = $"{FestivalCacheKeyPrefix}{internalFestivalName}";
            var cachedFestival = await _cacheService.GetAsync<FestivalData>(cacheKey);
            if (cachedFestival != null)
            {
                return cachedFestival;
            }

            var festival = await _clashFindersClient.GetFestival(internalFestivalName);
            var festivalData = MapToFestivalData(festival);
            await _cacheService.SetAsync(cacheKey, festivalData, FestivalDetailsCacheOptions);
            return festivalData;
        }

        public async Task<List<FestivalData>> GetFestivalsByYear(int year)
        {
            var allFestivalsList = await GetAllFestivals();
            var festivalsByYear = allFestivalsList
                .Where(f => f.StartDate.Year == year)
                .ToList();

            var tasks = festivalsByYear.Select(f => GetFestival(f.InternalName)).ToList();
            var festivalDataList = (await Task.WhenAll(tasks)).ToList();

            return festivalDataList;
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

        public async Task RefreshCacheAsync()
        {
            var festivals = await _clashFindersClient.GetAllFestivals();
            var festivalModels = festivals.Select(f => new FestivalListItemModel
            {
                Title = f.Title,
                InternalName = f.InternalName,
                StartDate = f.StartDate,
                PrintAdvisory = (int)f.PrintAdvisory
            }).ToList();

            await _cacheService.SetAsync(AllFestivalsCacheKey, festivalModels, FestivalListCacheOptions);

            await Parallel.ForEachAsync(festivalModels, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (festivalSummary, ct) =>
            {
                var festival = await _clashFindersClient.GetFestival(festivalSummary.InternalName);
                var festivalData = MapToFestivalData(festival);
                var cacheKey = $"{FestivalCacheKeyPrefix}{festivalSummary.InternalName}";
                await _cacheService.SetAsync(cacheKey, festivalData, FestivalDetailsCacheOptions);
                await Task.Delay(100, ct); // Optional: be gentle to clashfinder.com
            });
        }

        /// <summary>
        /// Maps external API model (Festival) to domain model (FestivalData)
        /// This is the single place where the coupling to ClashFinders API exists
        /// </summary>
        private static FestivalData MapToFestivalData(Festival festival)
        {
            var locations = festival.Locations.Select(loc => new LocationData(
                loc.Name,
                loc.Events.Select(e => new EventData(
                    e.Name,
                    e.Short,
                    e.Start,
                    e.End,
                    e.Estd
                )).ToList()
            )).ToList();

            return new FestivalData(
                festival.Id,
                festival.Name,
                festival.Url,
                festival.StartDate,
                locations
            );
        }
    }
}
