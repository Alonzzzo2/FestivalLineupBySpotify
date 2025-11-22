namespace Spotify_Alonzzo_API.Clients.Sporify.Models
{
    /// <summary>
    /// Artist model from Spotify API
    /// Contains only data returned by Spotify (name and number of liked tracks)
    /// </summary>
    public class Artist
    {
        public string Name { get; set; }
        public int NumOfLikedTracks { get; set; }

        public Artist(string name, int numOfLikedTracks)
        {
            Name = name;
            NumOfLikedTracks = numOfLikedTracks;
        }
    }
}