using FestivalMatcherAPI.Models;
using FestivalMatcherAPI.Clients.Spotify.Models;

namespace FestivalMatcherAPI.Services
{
    public class FestivalMatchingService : IFestivalMatchingService
    {
        private const int NumberOfHighlightGroups = 4;
        private static readonly char[] ArtistNameSeparators = [';', ',', '-', ' '];
        private readonly ISpotifyService _spotifyService;
        private readonly IClashFindersService _clashFindersService;
        private readonly IFestivalRankingService _rankingService;

        public FestivalMatchingService(
            ISpotifyService spotifyService,
            IClashFindersService clashFindersService,
            IFestivalRankingService rankingService)
        {
            _spotifyService = spotifyService;
            _clashFindersService = clashFindersService;
            _rankingService = rankingService;
        }

        public async Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYearForLikedSongs(int year)
        {
            var artistsFromLikedSongs = await _spotifyService.GetArtistsFromLikedSongs();
            var festivals = await _clashFindersService.GetFestivalsByYear(year);
            var results = await Task.WhenAll(
                festivals.Select(f => GetMatchedFestival(f, artistsFromLikedSongs, "liked songs"))
            );
            // Sort by tracks per show (primary), then matched tracks (secondary)
            return [.. results.OrderByDescending(r => r.TracksPerShow).ThenByDescending(r => r.MatchedTracks)];
        }

        public async Task<ClashFindersLinkModel> GetMatchedFestivalByNameForLikedSongs(string internalFestivalName)
        {
            var artistsFromLikedSongs = await _spotifyService.GetArtistsFromLikedSongs();
            var festival = await _clashFindersService.GetFestivalData(internalFestivalName);
            return await GetMatchedFestival(festival, artistsFromLikedSongs, "liked songs");
        }

        public async Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYearForPlaylist(
            string playlistUrl, 
            int year)
        {
            var playlistArtists = await _spotifyService.GetArtistsFromPublicPlaylist(playlistUrl);
            var festivals = await _clashFindersService.GetFestivalsByYear(year);
            var results = await Task.WhenAll(
                festivals.Select(f => GetMatchedFestival(f, playlistArtists, "playlist"))
            );
            // Sort by tracks per show (primary), then matched tracks (secondary)
            return [.. results.OrderByDescending(r => r.TracksPerShow).ThenByDescending(r => r.MatchedTracks)];
        }

        public async Task<ClashFindersLinkModel> GetMatchedFestivalByNameForPlaylist(
            string playlistUrl, 
            string internalFestivalName)
        {
            var playlistArtists = await _spotifyService.GetArtistsFromPublicPlaylist(playlistUrl);
            var festival = await _clashFindersService.GetFestivalData(internalFestivalName);
            return await GetMatchedFestival(festival, playlistArtists, "playlist");
        }

        private async Task<ClashFindersLinkModel> GetMatchedFestival(
            FestivalData festival,
            List<ArtistInfo> artistsToMatch,
            string sourceType)
        {
            // Convert domain EventData to domain EventInfo
            var festivalEventsInfo = festival.Locations
                .SelectMany(location => location.Events)
                .Select(e => new EventInfo(e.Name, e.Short))
                .ToList();

            var artistsWithEvents = GenerateArtistsWithEvents(artistsToMatch, festivalEventsInfo);
            
            // Organize artists by priority tiers for ClashFinders URL
            var artistsByPriority = OrganizeArtistsByPriority(artistsWithEvents);
            var url = _clashFindersService.BuildHighlightUrl(festival.Id, artistsByPriority);

            // Calculate tracks per show
            var matchedTracks = artistsWithEvents.Sum(a => a.NumOfLikedTracks);
            var festivalSize = festival.GetNumActs();
            var tracksPerShow = festivalSize > 0 ? (float)matchedTracks / festivalSize : 0;

            // Generate data-driven ranking message (pass pre-calculated tracksPerShow)
            var rankingMessage = _rankingService.GenerateRankingMessage(
                matchedTracks,
                artistsWithEvents.Count,
                tracksPerShow,
                sourceType);

            // Extract festival data to domain model
            var festivalInfo = new FestivalInfo(
                name: festival.Name,
                id: festival.Id,
                url: festival.Url,
                startDate: festival.StartDate,
                totalActs: festivalSize
            );

            return new ClashFindersLinkModel(
                url,
                artistsWithEvents,
                matchedTracks,
                artistsWithEvents.Count,
                tracksPerShow,
                rankingMessage,
                festivalInfo);
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
