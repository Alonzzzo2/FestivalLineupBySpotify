using Spotify_Alonzzo_API.Clients.ClashFinders.Models;

namespace Spotify_Alonzzo_API.Controllers.DTO
{
    public class FestivalListItemResponse
    {
        public string Title { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public PrintAdvisoryQuality PrintAdvisory { get; set; }
    }
}
