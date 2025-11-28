namespace FestivalMatcherAPI.Configuration
{
    public class CacheSettings
    {
        public string RefreshKey { get; set; } = string.Empty;
        public int LikedSongsTtlMinutes { get; set; } = 20;
        public int PlaylistTtlMinutes { get; set; } = 20;
        public int FestivalListTtlHours { get; set; } = 48;
        public int FestivalDetailsTtlHours { get; set; } = 48;
    }
}
