using Spotify_Alonzzo_API.Clients.Sporify.Models;

namespace FestivalLineupBySpotify_API.Models
{
    /// <summary>
    /// Artist with matched festival events
    /// Domain model combining artist data with festival event matches
    /// </summary>
    public class ArtistWithEvents
    {
        /// <summary>
        /// Artist name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Number of liked tracks for this artist
        /// </summary>
        public int NumOfLikedTracks { get; }

        /// <summary>
        /// Festival events that match this artist
        /// </summary>
        public List<Event> Events { get; }

        public ArtistWithEvents(string name, int numOfLikedTracks, List<Event> events)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NumOfLikedTracks = numOfLikedTracks;
            Events = events ?? new List<Event>();
        }
    }
}
