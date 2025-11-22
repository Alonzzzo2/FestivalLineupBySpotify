using FestivalLineupBySpotify_API.Models;

namespace FestivalLineupBySpotify_API.Services
{
    public interface IFestivalMatchingService
    {
        Task<List<ClashFindersLinkModel>> GetMatchedFestivalsByYear(int year, bool forceReloadArtistData = false);
        Task<ClashFindersLinkModel> GetMatchedFestivalByName(string internalFestivalName, bool forceReloadArtistData = false);
    }
}
