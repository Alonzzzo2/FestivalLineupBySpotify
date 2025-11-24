namespace FestivalLineupBySpotify_API.Models
{
    public class ClashFindersLinkModel
    {
        public string Url { get; }
        public List<ArtistWithEvents> MatchedArtists { get; }
        public int MatchedTracks { get; }
        public int MatchedArtistsCount { get; }
        public float TracksPerShow { get; }
        public string RankingMessage { get; }
        public FestivalInfo Festival { get; }

        public ClashFindersLinkModel(
            string url,
            List<ArtistWithEvents> matchedArtists,
            int matchedTracks,
            int matchedArtistsCount,
            float tracksPerShow,
            string rankingMessage,
            FestivalInfo festival)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            MatchedArtists = matchedArtists ?? throw new ArgumentNullException(nameof(matchedArtists));
            MatchedTracks = matchedTracks;
            MatchedArtistsCount = matchedArtistsCount;
            TracksPerShow = tracksPerShow;
            RankingMessage = rankingMessage ?? string.Empty;
            Festival = festival ?? throw new ArgumentNullException(nameof(festival));
        }
    }
}
