using FestivalLineupBySpotify_API.Services;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName, bool forceReloadData);
        Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear);
    }
}