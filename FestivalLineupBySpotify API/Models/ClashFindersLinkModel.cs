namespace FestivalLineupBySpotify_API.Models
{
    public class ClashFindersLinkModel
    {
        public string Url { get; }
        public int TotalPossibleLikedTracks { get; }
        public FestivalInfo Festival { get; }

        public ClashFindersLinkModel(string url, int totalPossibleLikedTracks, FestivalInfo festival)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            TotalPossibleLikedTracks = totalPossibleLikedTracks;
            Festival = festival ?? throw new ArgumentNullException(nameof(festival));
        }

        /// <summary>
        /// Match ranking: ratio of user's liked tracks to total acts
        /// Higher rank = more user's favorite artists performing
        /// </summary>
        public float Rank => Festival.TotalActs > 0 
            ? (float)TotalPossibleLikedTracks / Festival.TotalActs 
            : 0;
    }
}
