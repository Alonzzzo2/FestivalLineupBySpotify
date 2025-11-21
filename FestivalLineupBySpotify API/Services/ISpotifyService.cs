using FestivalLineupBySpotify_API.DTO;
using FestivalLineupBySpotify_API.Models;
using Spotify_Alonzzo_API.Clients.Models;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, Festival festival, bool forceReloadData = false);
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string internalFestivalName, bool forceReloadData = false);
        Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear);
        Task<List<FestivalListItemResponse>> GetAllFestivals();
    }
}