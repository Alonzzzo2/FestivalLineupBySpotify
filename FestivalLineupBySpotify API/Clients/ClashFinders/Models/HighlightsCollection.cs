using Microsoft.AspNetCore.WebUtilities;

namespace Spotify_Alonzzo_API.Clients.ClashFinders.Models
{
    /// <summary>
    /// Internal model for building ClashFinders highlight URLs
    /// Groups favorite artists into 4 priority tiers for visual highlighting
    /// ClashFinders-specific implementation for URL construction
    /// </summary>
    internal class HighlightsCollection
    {
        private readonly List<Highlight> _highlights;

        public HighlightsCollection()
        {
            _highlights = new List<Highlight>
            {
                new() { Index = 1 },
                new() { Index = 2 },
                new() { Index = 3 },
                new() { Index = 4 }
            };
        }

        /// <summary>
        /// Gets a highlight group by index (0-3)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is outside valid range</exception>
        public Highlight this[int index]
        {
            get
            {
                if (index < 0 || index >= _highlights.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Highlight index must be 0-3");
                return _highlights[index];
            }
        }

        /// <summary>
        /// Generate ClashFinders URL with highlighted artists
        /// </summary>
        /// <param name="festivalName">The festival identifier</param>
        /// <returns>ClashFinders URL with highlight parameters</returns>
        /// <exception cref="ArgumentNullException">Thrown when festivalName is null or empty</exception>
        public string GenerateUrl(string festivalName)
        {
            if (string.IsNullOrEmpty(festivalName))
                throw new ArgumentNullException(nameof(festivalName), "Festival name cannot be null or empty");

            var baseUrl = $"{ClashFindersConstants.BaseUrl}{string.Format(ClashFindersConstants.LineupUrlPattern, festivalName)}";
            
            // Build query parameters from active highlights
            var queryParams = new Dictionary<string, string>();
            foreach (var highlight in _highlights.Where(h => h.ArtistsShortEventNames.Count > 0))
            {
                var paramValue = string.Join(',', highlight.ArtistsShortEventNames);
                queryParams.Add($"hl{highlight.Index}", paramValue);
            }

            // Use QueryHelpers to properly build URL with parameters
            return QueryHelpers.AddQueryString(baseUrl, queryParams);
        }

        /// <summary>
        /// Internal highlight group for ClashFinders URL building
        /// Represents one of four priority tiers (hl1, hl2, hl3, hl4)
        /// </summary>
        internal class Highlight
        {
            /// <summary>
            /// Priority index (1-4) for this highlight group
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Short event names of artists in this priority tier
            /// </summary>
            public List<string> ArtistsShortEventNames { get; set; } = [];

            /// <summary>
            /// Format: hl{Index}=artist1,artist2,artist3
            /// </summary>
            public override string ToString() => $"hl{Index}={string.Join(',', ArtistsShortEventNames)}";
        }
    }
}
