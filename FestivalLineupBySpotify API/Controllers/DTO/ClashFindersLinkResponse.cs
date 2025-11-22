namespace Spotify_Alonzzo_API.Controllers.DTO
{
    public class ClashFindersLinkResponse
    {
        public string Url { get; set; }

        public int TotalPossibleLikedTracks { get; set; }

        public float Rank { get; set; }

        public FestivalResponse Festival { get; set; }

        public ClashFindersLinkResponse(string url, int totalPossibleLikedTracks, float rank, FestivalResponse festival)
        {
            Url = url;
            TotalPossibleLikedTracks = totalPossibleLikedTracks;
            Rank = rank;
            Festival = festival;
        }
    }
}
