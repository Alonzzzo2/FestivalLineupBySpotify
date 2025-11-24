namespace FestivalLineupBySpotify_API.Services
{
    /// <summary>
    /// Service for generating festival ranking messages
    /// </summary>
    public interface IFestivalRankingService
    {
        /// <summary>
        /// Generate a data-driven ranking message with only factual information
        /// </summary>
        /// <param name="matchedTracks">Number of user's tracks found at festival</param>
        /// <param name="matchedArtists">Number of user's artists performing</param>
        /// <param name="tracksPerShow">Pre-calculated average tracks per show</param>
        /// <param name="sourceType">Source type: "playlist" or "liked songs"</param>
        /// <returns>Data-driven ranking message</returns>
        string GenerateRankingMessage(
            int matchedTracks,
            int matchedArtists,
            float tracksPerShow,
            string sourceType = "playlist");
    }
}
