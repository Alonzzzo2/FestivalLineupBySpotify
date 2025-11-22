using FestivalLineupBySpotify_API.Models;
using Spotify_Alonzzo_API.Clients.Spotify.Models;
using Spotify_Alonzzo_API.Services;

namespace FestivalLineupBySpotify_API.Services
{
    public class FestivalMatchingService : IFestivalMatchingService
    {
        private const int NumberOfHighlightGroups = 4;
        private static readonly char[] ArtistNameSeparators = [';', ',', '-', ' '];
        private readonly ISpotifyService _spotifyService;
        private readonly IClashFindersService _clashFindersService;

        public FestivalMatchingService(ISpotifyService spotifyService, IClashFindersService clashFindersService)
        {
            _spotifyService = spotifyService;
            _clashFindersService = clashFindersService;
        }

        public async Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYear(int year, bool forceReloadArtistData = false)
        {
            var festivals = await _clashFindersService.GetFestivalsByYear(year);
            var results = await Task.WhenAll(
                festivals.Select(f => GetMatchedFestival(f, forceReloadArtistData))
            );
            return [.. results.OrderByDescending(r => r.Rank)];
        }

        public async Task<ClashFindersLinkModel> GetMatchedFestivalByName(string internalFestivalName, bool forceReloadArtistData = false)
        {
            var festival = await _clashFindersService.GetFestival(internalFestivalName);
            return await GetMatchedFestival(festival, forceReloadArtistData);
        }

        private async Task<ClashFindersLinkModel> GetMatchedFestival(FestivalData festival, bool forceReloadArtistData = false)
        {
            var favoriteArtists = await _spotifyService.GetFavoriteArtists(forceReloadArtistData);

            // Convert domain EventData to domain EventInfo
            var festivalEventsInfo = festival.Locations
                .SelectMany(location => location.Events)
                .Select(e => new EventInfo(e.Name, e.Short))
                .ToList();

            var artistsWithEvents = GenerateArtistsWithEvents(favoriteArtists, festivalEventsInfo);
            
            // Organize artists by priority tiers
            var artistsByPriority = OrganizeArtistsByPriority(artistsWithEvents);
            
            // Let ClashFindersService handle URL building
            var url = _clashFindersService.BuildHighlightUrl(festival.Id, artistsByPriority);

            // Extract festival data to domain model
            var festivalInfo = new FestivalInfo(
                name: festival.Name,
                id: festival.Id,
                url: festival.Url,
                printAdvisory: festival.PrintAdvisory,
                modified: festival.Modified,
                startDateUnix: festival.StartDateUnix,
                totalActs: festival.GetNumActs()
            );

            var result = new ClashFindersLinkModel(url, artistsWithEvents.Sum(a => a.NumOfLikedTracks), festivalInfo);
            return result;
        }

        private List<string>[] OrganizeArtistsByPriority(List<ArtistWithEvents> artists)
        {
            var artistsByPriority = new List<string>[NumberOfHighlightGroups];
            for (int i = 0; i < artistsByPriority.Length; i++)
            {
                artistsByPriority[i] = new List<string>();
            }

            double groupSize = (double)artists.Count / (double)NumberOfHighlightGroups;
            for (int i = 0; i < artists.Count; i++)
            {
                int priorityTier = (int)(Math.Floor((double)(i / groupSize)));
                var clashFindersShortArtistsNames = artists[i].Events
                    .Select(e => (e.Short ?? string.Empty).Split('(')[0])
                    .Distinct()
                    .ToList();
                artistsByPriority[priorityTier].AddRange(clashFindersShortArtistsNames);
            }

            return artistsByPriority;
        }

        private List<ArtistWithEvents> GenerateArtistsWithEvents(List<ArtistInfo> artists, List<EventInfo> allFestivalEvents)
        {
            // For each favorite artist, if they have a festival event, create a match result
            var artistsWithEvents = artists
                .Select(artist =>
                {
                    var artistNames = artist.Name.ToLower().Split(" ");
                    var matchedEvents = allFestivalEvents
                        .Where(festivalEvent => DoesEventBelongToArtist(festivalEvent, artistNames))
                        .ToList();

                    return new ArtistWithEvents(
                        artist.Name,
                        artist.NumOfLikedTracks,
                        matchedEvents
                    );
                })
                .Where(matchResult => matchResult.Events.Any())
                .OrderByDescending(a => a.NumOfLikedTracks)
                .ToList();

            return artistsWithEvents;
        }

        private bool DoesEventBelongToArtist(EventInfo festivalEvent, string[] artistNames)
        {
            var festivalEventArtists = festivalEvent.Name.ToLower().Split(ArtistNameSeparators, StringSplitOptions.RemoveEmptyEntries);
            return artistNames.All(artistName => festivalEventArtists.Contains(artistName));            
        }
    }
}
