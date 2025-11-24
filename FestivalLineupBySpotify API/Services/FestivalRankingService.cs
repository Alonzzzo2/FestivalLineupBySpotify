namespace FestivalLineupBySpotify_API.Services
{
    /// <summary>
    /// Generates data-driven ranking messages for festival matches
    /// </summary>
    public class FestivalRankingService : IFestivalRankingService
    {
        /// <summary>
        /// Generate a data-driven ranking message with only factual information
        /// </summary>
        public string GenerateRankingMessage(
            int matchedTracks,
            int matchedArtists,
            float tracksPerShow,
            string sourceType = "playlist")
        {
            if (matchedTracks == 0 || matchedArtists == 0)
                return $"0 potential tracks from your {sourceType} at this festival.";

            // Build factual message with only data
            var trackWord = matchedTracks == 1 ? "track" : "tracks";
            var artistWord = matchedArtists == 1 ? "artist" : "artists";

            // Pure data: count + per-show metric
            var message = $"{matchedTracks} potential {trackWord} across {matchedArtists} {artistWord} from your {sourceType}";

            // Add per-show statistic
            if (tracksPerShow > 0)
            {
                message += $", {tracksPerShow:F2} per show";
            }

            message += ".";

            return message;
        }
    }
}
