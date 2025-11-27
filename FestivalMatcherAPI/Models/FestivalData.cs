namespace FestivalMatcherAPI.Models
{
    /// <summary>
    /// Domain model representing a complete festival with all its lineup details.
    /// This is the internal representation independent of external API models.
    /// </summary>
    public class FestivalData
    {
        public string Id { get; }
        public string Name { get; }
        public string Url { get; }
        public DateTime StartDate { get; }
        public List<LocationData> Locations { get; }

        public FestivalData(string id, string name, string url, DateTime startDate, List<LocationData> locations)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            StartDate = startDate;
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
        }

        /// <summary>
        /// Gets the total number of acts by counting all events across all locations
        /// </summary>
        public int GetNumActs()
        {
            return Locations.SelectMany(l => l.Events).Count();
        }
    }

    /// <summary>
    /// Domain model representing a location/stage within a festival
    /// </summary>
    public class LocationData
    {
        public string Name { get; }
        public List<EventData> Events { get; }

        public LocationData(string name, List<EventData> events)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }
    }

    /// <summary>
    /// Domain model representing an event/performance at a festival
    /// </summary>
    public class EventData
    {
        public string Name { get; }
        public string Short { get; }
        public string Start { get; }
        public string End { get; }
        public string Estd { get; }

        public EventData(string name, string @short, string start, string end, string estd)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Short = @short ?? string.Empty;
            Start = start ?? string.Empty;
            End = end ?? string.Empty;
            Estd = estd ?? string.Empty;
        }
    }
}
