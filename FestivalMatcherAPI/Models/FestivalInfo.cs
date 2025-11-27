namespace FestivalMatcherAPI.Models
{
    public class FestivalInfo
    {
        public string Name { get; }
        public string Id { get; }
        public string Url { get; }
        public DateTime StartDate { get; }
        public int TotalActs { get; }

        public FestivalInfo(string name, string id, string url, DateTime startDate, int totalActs)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            StartDate = startDate;
            TotalActs = totalActs;
        }
    }
}
