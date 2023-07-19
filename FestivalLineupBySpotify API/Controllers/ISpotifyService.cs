using FestivalLineupBySpotify_API.Services;

namespace FestivalLineupBySpotify_API.Controllers
{
    public interface ISpotifyService
    {
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName);
        Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear);
    }
}