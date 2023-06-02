namespace FestivalLineupBySpotify_API.Services
{
    public class ClashFindersFavoritesResult
    {
        public string Url { get; set; }
        public int TotalPossibleLikesTracks { get; set; }
        public ClashFindersFavoritesResult(string url, int totalPossibleLikesTracks)
        {
            Url = url;
            TotalPossibleLikesTracks = totalPossibleLikesTracks;
        }
    }
}
