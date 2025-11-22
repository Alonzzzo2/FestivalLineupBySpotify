using FestivalLineupBySpotify_API.Models;
using Spotify_Alonzzo_API.Clients.Spotify.Models;

namespace FestivalLineupBySpotify_API.Services
{
    public interface IClashFindersService
    {
        Task<List<FestivalListItemModel>> GetAllFestivals();
        Task<Festival> GetFestival(string internalFestivalName);
        Task<List<Festival>> GetFestivalsByYear(int year);
        
        /// <summary>
        /// Build a ClashFinders highlight URL for a festival with matched artists organized by priority
        /// </summary>
        /// <param name="festivalId">The festival identifier</param>
        /// <param name="artistsByPriority">Array of artist lists, one per priority tier (up to 4)</param>
        /// <returns>ClashFinders URL with highlight parameters</returns>
        string BuildHighlightUrl(string festivalId, List<string>[] artistsByPriority);
    }
}
