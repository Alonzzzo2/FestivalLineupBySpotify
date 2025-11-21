using Newtonsoft.Json;

namespace FestivalLineupBySpotify_API.DTO
{
    public class ClashFindersFavoritesResponse
    {
        public string Url { get; set; }

        public int TotalPossibleLikedTracks { get; set; }

        public float Rank { get; set; }

        public FestivalResponse Festival { get; set; }

        public ClashFindersFavoritesResponse(string url, int totalPossibleLikedTracks, float rank, FestivalResponse festival)
        {
            Url = url;
            TotalPossibleLikedTracks = totalPossibleLikedTracks;
            Rank = rank;
            Festival = festival;
        }
    }
}
