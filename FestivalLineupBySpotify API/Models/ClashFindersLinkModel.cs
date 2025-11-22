using Newtonsoft.Json;
using Spotify_Alonzzo_API.Clients.Sporify.Models;

namespace FestivalLineupBySpotify_API.Models
{
    public class ClashFindersLinkModel
    {
        public string Url { get; set; }

        public int TotalPossibleLikedTracks { get; set; }

        public Festival Festival { get; set; }

        public ClashFindersLinkModel(string url, int totalPossibleLikesTracks, Festival festival)
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
