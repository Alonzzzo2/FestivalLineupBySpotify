using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace FestivalLineupBySpotify_API.DTO
{
    public class FestivalEvent
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public int NumActs { get; set; }
        public DateTime StartDate { get; set; }

        public FestivalEvent(JProperty property)
        {
            Name = property.Value["name"].ToString();
            Desc = property.Value["desc"].ToString();
            NumActs = Convert.ToInt32(property.Value["numActs"]);
            StartDate = DateTimeOffset.FromUnixTimeSeconds((long)(property.Value["startDate"] ?? 0)).UtcDateTime;
        }
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

    public class Root
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

        [JsonProperty("locations")]
        public List<Location> Locations { get; set; }
    }
}