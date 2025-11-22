using Newtonsoft.Json;

namespace Spotify_Alonzzo_API.Clients.Spotify.Models
{
    public class Festival
    {
        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("printAdvisory")]
        public int PrintAdvisory { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("tzOffset")]
        public int TzOffset { get; set; }

        [JsonProperty("tzNote")]
        public string TzNote { get; set; }

        [JsonProperty("startDate")]
        public long StartDateUnix { get; set; }

        [JsonProperty("locations")]
        public required List<Location> Locations { get; set; }

        /// <summary>
        /// Gets the start date converted from Unix timestamp
        /// </summary>
        public DateTime StartDate => DateTimeOffset.FromUnixTimeSeconds(StartDateUnix).UtcDateTime;

        /// <summary>
        /// Gets the total number of acts by counting all events across all locations
        /// </summary>
        public int GetNumActs()
        {
            int totalActs = 0;
            if (Locations != null)
            {
                foreach (var location in Locations)
                {
                    if (location?.Events != null)
                    {
                        totalActs += location.Events.Count;
                    }
                }
            }
            return totalActs;
        }

        // Conditional serialization methods
        public bool ShouldSerializeCopyright() => false;
        public bool ShouldSerializeTimezone() => false;
        public bool ShouldSerializeTzOffset() => false;
        public bool ShouldSerializeTzNote() => false;
        public bool ShouldSerializeLocations() => false;
    }
    public class Event
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("short")]
        public string Short { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("estd")]
        public string Estd { get; set; }
    }

    public class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("events")]
        public List<Event> Events { get; set; }
    }
}