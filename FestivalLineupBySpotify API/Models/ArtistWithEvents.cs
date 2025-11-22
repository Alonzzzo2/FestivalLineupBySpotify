namespace FestivalLineupBySpotify_API.Models
{
    public class ArtistWithEvents
    {
        public string Name { get; }
        public int NumOfLikedTracks { get; }
        public List<EventInfo> Events { get; }

        public ArtistWithEvents(string name, int numOfLikedTracks, List<EventInfo> events)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NumOfLikedTracks = numOfLikedTracks;
            Events = events ?? new List<EventInfo>();
        }
    }
}
