namespace FestivalLineupBySpotify_API.Constants
{
    public static class AuthenticationConstants
    {
        public const string FrontendUrlConfigKey = "FrontendUrl";
        public static readonly TimeSpan TokenExpiration = TimeSpan.FromHours(1);
        public const string FavoriteArtistsCacheKey = "favorite_artists";
    }
}