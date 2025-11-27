namespace FestivalMatcherAPI.Controllers.DTO
{
    public class ClashFindersLinkResponse
    {
        public string Url { get; set; }

        public int MatchedArtistsCount { get; set; }

        public int MatchedTracksCount { get; set; }

        public float TracksPerShow { get; set; }

        public string RankingMessage { get; set; }

        public FestivalResponse Festival { get; set; }

        public ClashFindersLinkResponse(
            string url,
            int matchedArtistsCount,
            int matchedTracksCount,
            float tracksPerShow,
            string rankingMessage,
            FestivalResponse festival)
        {
            Url = url;
            MatchedArtistsCount = matchedArtistsCount;
            MatchedTracksCount = matchedTracksCount;
            TracksPerShow = tracksPerShow;
            RankingMessage = rankingMessage ?? string.Empty;
            Festival = festival;
        }
    }
}
