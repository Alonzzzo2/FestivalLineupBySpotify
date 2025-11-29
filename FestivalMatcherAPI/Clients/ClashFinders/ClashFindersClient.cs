using Newtonsoft.Json;
using FestivalMatcherAPI.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using FestivalMatcherAPI.Clients.ClashFinders;
using FestivalMatcherAPI.Clients.ClashFinders.Models;
using FestivalMatcherAPI.Clients.Spotify.Models;

namespace FestivalMatcherAPI.Clients.ClashFinders
{
    public class ClashFindersClient
    {
        private readonly string _authUsername;
        private readonly string _authPublicKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClashFindersClient> _logger;

        public ClashFindersClient(
            IOptions<ClashFindersSettings> options, 
            HttpClient httpClient,
            ILogger<ClashFindersClient> logger)
        {
            _httpClient = httpClient;
            _authUsername = options.Value.AuthUsername;
            _authPublicKey = options.Value.AuthPublicKey;
            _logger = logger;
        }

        private static string LineupUrl(string eventName) => $"{ClashFindersConstants.BaseUrl}{string.Format(ClashFindersConstants.LineupUrlPattern, eventName)}";

        private string LineupDataUrl(string eventName) =>
            $"{string.Format(ClashFindersConstants.LineupDataPattern, eventName)}?authUsername={Uri.EscapeDataString(_authUsername)}&authPublicKey={Uri.EscapeDataString(_authPublicKey)}";

        public async Task<Festival> GetFestival(string internalFestivalName) =>
            JsonConvert.DeserializeObject<Festival>(await _httpClient.GetStringAsync(LineupDataUrl(internalFestivalName)))
                ?? throw new InvalidOperationException("Empty events json data!");

        public async Task<List<FestivalListItem>> GetAllFestivals()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(await _httpClient.GetStringAsync(ClashFindersConstants.FestivalListEndpoint));

            var listItems = doc.DocumentNode.SelectNodes("//tbody[@class='cfListItem']") ?? new HtmlNodeCollection(null);

            if (listItems.Count == 0)
                return new List<FestivalListItem>();

            return listItems
                .Select(item =>
                {
                    try
                    {
                        var titleNode = item.SelectSingleNode(".//a[@class='cfTitle']");
                        var nameNode = item.SelectSingleNode(".//a[@class='cfName']");
                        var dateNode = item.SelectSingleNode(".//td[@class='cfStartDate']");
                        var printAdvNode = item.SelectSingleNode(".//td[@class='cfPrintAdv']");

                        if (titleNode == null || nameNode == null || dateNode == null)
                            return null;

                        var title = titleNode.InnerText?.Trim() ?? string.Empty;
                        var internalName = nameNode.InnerText?.Trim() ?? string.Empty;
                        var dateText = dateNode.InnerText?.Trim() ?? string.Empty;
                        var dateMatch = System.Text.RegularExpressions.Regex.Match(dateText, @"^\d{4}-\d{1,2}-\d{1,2}");
                        dateText = dateMatch.Success ? dateMatch.Value : dateText.Split('\n')[0]?.Trim() ?? string.Empty;

                        return DateTime.TryParse(dateText, out var startDate)
                            ? new FestivalListItem
                            {
                                Title = title,
                                InternalName = internalName,
                                StartDate = startDate,
                                PrintAdvisory = ParsePrintAdvisory(printAdvNode?.InnerText?.Trim() ?? string.Empty)
                            }
                            : null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse festival list item. Skipping.");
                        return null;
                    }
                })
                .Where(f => f != null)
                .Cast<FestivalListItem>()
                .Where(item => 
                    item.PrintAdvisory == PrintAdvisoryQuality.HaveAtIt ||
                    item.PrintAdvisory == PrintAdvisoryQuality.PrintIfYouHaveTo)
                .Where(item => item.StartDate > DateTime.Today.AddYears(-3))
                .ToList();
        }

        private static PrintAdvisoryQuality ParsePrintAdvisory(string text) =>
            string.IsNullOrEmpty(text) ? PrintAdvisoryQuality.Unknown :
            int.TryParse(text[0].ToString(), out var quality) && quality >= 1 && quality <= 6
                ? (PrintAdvisoryQuality)quality
                : PrintAdvisoryQuality.Unknown;
    }
}
