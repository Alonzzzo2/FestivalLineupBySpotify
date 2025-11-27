namespace FestivalMatcherAPI.Clients.ClashFinders.Models
{
    public enum PrintAdvisoryQuality
    {
        Unknown = 0,
        HaveAtIt = 1,
        PrintIfYouHaveTo = 2,
        NotNowSoonIPromise = 3,
        Unknown4 = 4,
        DontEvenThinkAboutIt = 5,
        FantasyIsland = 6
    }

    public class FestivalListItem
    {
        public string Title { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public PrintAdvisoryQuality PrintAdvisory { get; set; }
    }
}
