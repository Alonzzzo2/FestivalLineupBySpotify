using Spotify_Alonzzo_API.Clients.ClashFinders.Models;

namespace FestivalLineupBySpotify_API.Models
{
    public class FestivalListItemModel
    {
        public string Title { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public PrintAdvisoryQuality PrintAdvisory { get; set; }
    }
}
