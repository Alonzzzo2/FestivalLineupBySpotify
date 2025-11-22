namespace FestivalLineupBySpotify_API.Models
{
    public class FestivalInfo
    {
        public string Name { get; }
        public string Id { get; }
        public string Url { get; }
        public int PrintAdvisory { get; }
        public string Modified { get; }
        public long StartDateUnix { get; }
        public int TotalActs { get; }

        public FestivalInfo(string name, string id, string url, int printAdvisory, string modified, long startDateUnix, int totalActs)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            PrintAdvisory = printAdvisory;
            Modified = modified ?? string.Empty;
            StartDateUnix = startDateUnix;
            TotalActs = totalActs;
        }
    }
}
