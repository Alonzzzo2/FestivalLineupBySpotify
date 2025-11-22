namespace FestivalLineupBySpotify_API.Models
{
    public class EventInfo
    {
        public string Name { get; }
        public string Short { get; }

        public EventInfo(string name, string @short)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Short = @short ?? string.Empty;
        }
    }
}
