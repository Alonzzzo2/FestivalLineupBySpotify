using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace FestivalLineupBySpotify_API.DTO
{
    public class FestivalEvent
    {
        private readonly string _name;
        private readonly string _desc;
        private readonly long _startDate;
        private readonly int _numActs;
        public FestivalEvent(JProperty property)
        {
            this._name = property.Value["name"].ToString();
            this._desc = property.Value["desc"].ToString();
            this._numActs = Convert.ToInt32(property.Value["numActs"]);
            this._startDate = (long)Convert.ToDouble(property.Value["startDate"]);
        }

        public string Name => _name;
        public string Desc => _desc;
        public int NumActs => _numActs;
        public DateTime StartDate {get {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(this._startDate);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            return dateTime;
        }}
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