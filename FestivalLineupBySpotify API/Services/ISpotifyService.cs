using FestivalLineupBySpotify_API.Services;

namespace Spotify_Alonzzo_API.Services
{
    public interface ISpotifyService
    {
        Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName);
        Task<List<ClashFindersFavoritesResult>> GenerateClashFindersFavoritesResult(HttpRequest request, int festivalsYear);
    }
}