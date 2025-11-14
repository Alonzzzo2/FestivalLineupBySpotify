using Newtonsoft.Json;
using FestivalLineupBySpotify_API.Configuration;
using FestivalLineupBySpotify_API.DTO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace FestivalLineupBySpotify_API.Services
{
    public class ClashFindersService
    {
        private const string ClashFindersUrl = "https://clashfinder.com";
        private const string AllEventsUrl = "https://clashfinder.com/data/events/events.json";
        
        private readonly string _authUsername;
        private readonly string _authPublicKey;
        private readonly HttpClient _httpClient;

        public ClashFindersService(IOptions<ClashFindersSettings> options, HttpClient httpClient)
        {
            _authUsername = options.Value.AuthUsername ?? string.Empty;
            _authPublicKey = options.Value.AuthPublicKey ?? string.Empty;
            _httpClient = httpClient;
        }

        private static string LineupUrl(string eventName) => $"{ClashFindersUrl}/s/{eventName}";

        private string LineupDataUrl(string eventName)
        {
            var url = $"{ClashFindersUrl}/data/event/{eventName}.json";
            if (!string.IsNullOrEmpty(_authUsername) && !string.IsNullOrEmpty(_authPublicKey))
            {
                url += $"?authUsername={System.Uri.EscapeDataString(_authUsername)}&authPublicKey={System.Uri.EscapeDataString(_authPublicKey)}";
            }
            return url;
        }

        public async Task<List<Event>> GetEventsFromClashFinders(string festivalName)
        {
            var lineupDataUrlForFestival = LineupDataUrl(festivalName);
            var response = await _httpClient.GetAsync(lineupDataUrlForFestival);
            var contentStream = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<Root>(contentStream);
            
            if (rootData == null)
            {
                throw new Exception("Empty events json data!");
            }

            return rootData.Locations.SelectMany(location => location.Events).ToList();
        }

        public async Task<List<FestivalEvent>> GetAllEventsByYear(int year)
        {
            var response = await _httpClient.GetAsync(AllEventsUrl);
            var contentStream = await response.Content.ReadAsStringAsync();

            JObject json = JObject.Parse(contentStream);
            var festivals = json.Properties().Select(p => new FestivalEvent(p)).Where(f => f.StartDate.Year == year).ToList();
            return festivals;
        }

        public class Highlight
        {
            public int Index { get; set; }
            public List<string> ArtistsShortEventNames { get; set; } = new List<string>();

            public override string ToString()
            {
                return $"hl{Index}={string.Join(',', this.ArtistsShortEventNames)}";
            }
        }

        public class HighlightsCollection
        {
            public List<Highlight> Highlights { get; set; }
            public HighlightsCollection()
            {
                this.Highlights = new List<Highlight>()
                {
                    new Highlight() { Index = 1},
                    new Highlight() { Index = 2},
                    new Highlight() { Index = 3},
                    new Highlight() { Index = 4}
                };
            }

            public Highlight this[int index] => this.Highlights[index];

            public string GenerateUrl(string festivalName) 
            {
                var highlightsAsParameter = this.Highlights.Select(highlight => highlight.ToString());
                var url = $"{LineupUrl(festivalName)}/?{string.Join('&', highlightsAsParameter)}";
                return url;
            }

        }

        public class ClashFindersFavoritesURL
        {
            public string Url { get; set; } = string.Empty;
            public int Score { get; set; }
        }
    }
}
