using FestivalLineupBySpotify_API.Services;

namespace FestivalLineupBySpotify_API.Controllers
{
    public interface ISpotifyService
    {
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName);
    }
}