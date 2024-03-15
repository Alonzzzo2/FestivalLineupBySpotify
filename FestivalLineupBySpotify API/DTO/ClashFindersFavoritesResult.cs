using FestivalLineupBySpotify_API.DTO;
using Newtonsoft.Json;

namespace FestivalLineupBySpotify_API.Services
{
    public class ClashFindersFavoritesResult
    {
        public string Url { get; set; }

        public int TotalPossibleLikedTracks { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FestivalEvent? FestivalEvent { get; set; }

        public ClashFindersFavoritesResult(string url, int totalPossibleLikesTracks, FestivalEvent? festivalEvent = null)
        {
            Url = url;
            TotalPossibleLikedTracks = totalPossibleLikesTracks;
            FestivalEvent = festivalEvent;
        }

        public float Rank {get {
            if (FestivalEvent == null) return 0;
            return (float)TotalPossibleLikedTracks / FestivalEvent.NumActs;
        }}
    }
}
