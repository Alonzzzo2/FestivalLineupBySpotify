namespace FestivalLineupBySpotify_API.DTO
{
    public class Artist
    {
        public string Name { get; set; }
        public int NumOfLikedTracks { get; set; }
        public List<Event> Events { get; set; }

        public Artist(string Name, int numOfLikedTracks)
        {
            this.Name = Name;
            this.NumOfLikedTracks = numOfLikedTracks;
            this.Events = new List<Event>();
        }
    }
}