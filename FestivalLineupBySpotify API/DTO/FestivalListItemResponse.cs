using FestivalLineupBySpotify_API.Models;

namespace FestivalLineupBySpotify_API.DTO
{
    public class FestivalListItemResponse
    {
        public string Title { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public PrintAdvisoryQuality PrintAdvisory { get; set; }
    }
}
