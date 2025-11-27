using Newtonsoft.Json;

namespace FestivalMatcherAPI.Controllers.DTO
{
    public class FestivalResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }
    }
}
