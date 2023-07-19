using FestivalLineupBySpotify_API.DTO;
using Newtonsoft.Json;

namespace FestivalLineupBySpotify_API.Services
{
    public class ClashFindersFavoritesResult
    {
        public string Url { get; set; }

        public int TotalPossibleLikesTracks { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FestivalEvent? FestivalEvent { get; set; }

        public ClashFindersFavoritesResult(string url, int totalPossibleLikesTracks, FestivalEvent? festivalEvent = null)
        {
            Url = url;
            TotalPossibleLikesTracks = totalPossibleLikesTracks;
            FestivalEvent = FestivalEvent;
        }

        public float Rank {get {
            if (FestivalEvent == null) return 0;
            return (float)TotalPossibleLikesTracks / FestivalEvent.NumActs;
        }}
    }
}
