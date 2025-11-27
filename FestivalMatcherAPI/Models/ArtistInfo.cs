namespace FestivalMatcherAPI.Models
{
    public class ArtistInfo
    {
        public string Name { get; }
        public int NumOfLikedTracks { get; }

        public ArtistInfo(string name, int numOfLikedTracks)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NumOfLikedTracks = numOfLikedTracks;
        }
    }
}
