using Newtonsoft.Json;
using FestivalLineupBySpotify_API.DTO;
using Newtonsoft.Json.Linq;

namespace FestivalLineupBySpotify_API.Services
{
    public static class ClashFindersService
    {
        private static string clashFindersUrl = "https://clashfinder.com";

        // Read auth parameters from environment
        private static readonly string authUsername = Environment.GetEnvironmentVariable("AUTH_USERNAME");
        private static readonly string authPublicKey = Environment.GetEnvironmentVariable("AUTH_PUBLIC_KEY");

        private static string lineupUrl(string eventName) => $"{clashFindersUrl}/s/{eventName}";

        // Builds the data URL and appends auth query params when available
        private static string lineupDataUrl(string eventName)
        {
            var url = $"{clashFindersUrl}/data/event/{eventName}.json";
            if (!string.IsNullOrEmpty(authUsername) && !string.IsNullOrEmpty(authPublicKey))
            {
                url += $"?authUsername={System.Uri.EscapeDataString(authUsername)}&authPublicKey={System.Uri.EscapeDataString(authPublicKey)}";
            }
            return url;
        }

        private static string allEventsUrl = "https://clashfinder.com/data/events/events.json";

        public static async Task<List<Event>> GetEventsFromClashFinders(string festivalName)
        {
            var linupDataUrlForFestival = lineupDataUrl(festivalName);
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(linupDataUrlForFestival);
            var contentStream = await response.Content.ReadAsStringAsync();
            Root? rootData = JsonConvert.DeserializeObject<Root>(contentStream);
            if (rootData == null)
            {
                throw new Exception("Empty events json data!");
            }

            var events = rootData.Locations.SelectMany(location => location.Events).ToList();
            return events;
        }

        public static async Task<List<FestivalEvent>> GetAllEventsByYear(int year)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(allEventsUrl);
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
                var url = $"{lineupUrl(festivalName)}/?{string.Join('&', highlightsAsParameter)}";
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
