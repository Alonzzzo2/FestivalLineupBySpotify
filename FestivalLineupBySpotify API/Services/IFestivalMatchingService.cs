using FestivalLineupBySpotify_API.Models;

namespace FestivalLineupBySpotify_API.Services
{
    public interface IFestivalMatchingService
    {
        Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYearForLikedSongs(int year);
        
        Task<ClashFindersLinkModel> GetMatchedFestivalByNameForLikedSongs(string internalFestivalName);
        
        Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYearForPlaylist(
            string playlistUrl, 
            int year);

        Task<ClashFindersLinkModel> GetMatchedFestivalByNameForPlaylist(
            string playlistUrl, 
            string internalFestivalName);
    }
}
