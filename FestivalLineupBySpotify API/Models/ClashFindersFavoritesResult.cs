using Newtonsoft.Json;
using Spotify_Alonzzo_API.Clients.Models;

namespace FestivalLineupBySpotify_API.Models
{
    public class ClashFindersFavoritesResult
    {
        public string Url { get; set; }

        public int TotalPossibleLikedTracks { get; set; }

        public Festival Festival { get; set; }

        public ClashFindersFavoritesResult(string url, int totalPossibleLikesTracks, Festival festival)
        {
            Url = url;
            TotalPossibleLikedTracks = totalPossibleLikesTracks;
            Festival = festival;
        }

        public float Rank {get {
            if (Festival == null) return 0;
            return (float)TotalPossibleLikedTracks / Festival.GetNumActs();
        }}
    }
}
