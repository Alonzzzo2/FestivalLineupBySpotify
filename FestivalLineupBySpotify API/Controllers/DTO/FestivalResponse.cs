using Newtonsoft.Json;

namespace Spotify_Alonzzo_API.Controllers.DTO
{
    public class FestivalResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("printAdvisory")]
        public int PrintAdvisory { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("startDate")]
        public long StartDateUnix { get; set; }

        /// <summary>
        /// Gets the start date converted from Unix timestamp
        /// </summary>
        [JsonIgnore]
        public DateTime StartDate => DateTimeOffset.FromUnixTimeSeconds(StartDateUnix).UtcDateTime;
    }
}
