namespace FestivalMatcherAPI.Clients.ClashFinders
{
    /// <summary>
    /// Constants for ClashFinders API and URL building
    /// </summary>
    internal static class ClashFindersConstants
    {
        /// <summary>
        /// Base URL for ClashFinders service
        /// </summary>
        public const string BaseUrl = "https://clashfinder.com";

        /// <summary>
        /// Endpoint for retrieving all festivals
        /// </summary>
        public const string AllEventsEndpoint = "/data/events/events.json";

        /// <summary>
        /// Path pattern for festival lineup data
        /// Format: /data/event/{festivalName}.json
        /// </summary>
        public const string LineupDataPattern = "/data/event/{0}.json";

        /// <summary>
        /// Path pattern for festival lineup URL
        /// Format: /s/{festivalName}
        /// </summary>
        public const string LineupUrlPattern = "/s/{0}";

        /// <summary>
        /// Endpoint for festival list page
        /// </summary>
        public const string FestivalListEndpoint = "/list/?onlyTable=true&l=all";
    }
}
