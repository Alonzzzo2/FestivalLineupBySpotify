using FestivalMatcherAPI.Configuration;
using FestivalMatcherAPI.Services;
using FestivalMatcherAPI.Services.Caching;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using FestivalMatcherAPI.Clients.ClashFinders;
using FestivalMatcherAPI.Clients.Spotify;
using FestivalMatcherAPI.Controllers.DTO;

var builder = WebApplication.CreateBuilder(args);

// Configure settings from appsettings
builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<ClashFindersSettings>(builder.Configuration.GetSection("ClashFinders"));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("Cors"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

// Prefer configuration-driven URLs. Only bind Kestrel to a specific port
// when the environment or configuration provides a numeric PORT (e.g., in containers/PAAS).
var portValue = builder.Configuration["PORT"] ?? Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portValue) && int.TryParse(portValue, out var port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

// Add services to the container.

// Configure Data Protection with persistent key storage
builder.Services.AddDataProtection()
    .SetApplicationName("FestivalMatcher")
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(redisConnection))
    {
        throw new InvalidOperationException(
            "Redis connection string is not configured. Please set ConnectionStrings:Redis in appsettings.");
    }

    // Sanitize connection string: StackExchange.Redis works best with "host:port" rather than "redis://host:port"
    // Also handle potential double-port issues if they exist in the env var
    var cleanConnection = redisConnection.Replace("redis://", "", StringComparison.OrdinalIgnoreCase);
    
    // If the user somehow has host:6379:6379, let's try to fix it blindly if it ends with :6379:6379
    if (cleanConnection.EndsWith(":6379:6379"))
    {
        cleanConnection = cleanConnection.Substring(0, cleanConnection.Length - 5);
    }

    options.Configuration = cleanConnection;
    options.InstanceName = "FestivalMatcher:";
});
builder.Services.AddScoped<ICacheService, DistributedCacheService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<GetFestivalsWithPlaylistRequest>();

builder.Services.AddControllers();
// cache for liked songs, playlists and clashfinder data
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FestivalMatcherAPI.Swagger.AddAuthorizationHeaderForCacheRefreshOperationFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISpotifyClientAdapter, SpotifyClientAdapter>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddScoped<IClashFindersService, ClashFindersService>();
builder.Services.AddScoped<IFestivalRankingService, FestivalRankingService>();
builder.Services.AddScoped<IFestivalMatchingService, FestivalMatchingService>();
builder.Services.AddScoped<SpotifyAPI.Web.OAuthClient>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddHttpClient<ClashFindersClient>()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://clashfinder.com"));

// Add CORS using configuration
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (corsSettings?.AllowedOrigins?.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting FestivalMatcher API");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Port: {Port}", portValue ?? "Not configured (using default)");

var cors = builder.Configuration.GetSection("Cors").Get<CorsSettings>();
if (cors?.AllowedOrigins != null)
{
    logger.LogInformation("Allowed CORS Origins: {Origins}", string.Join(", ", cors.AllowedOrigins));
}
else
{
    logger.LogWarning("No CORS origins configured");
}

var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConn))
{
    // Mask the password/host for safety if needed, or just log existence
    // Simple masking: show only the beginning
    var maskedRedis = redisConn.Length > 10 ? redisConn.Substring(0, 10) + "..." : "Set but short";
    logger.LogInformation("Redis Connection: {RedisConn}", maskedRedis);
}
else
{
    logger.LogError("Redis connection string is MISSING");
}

var spotifySettings = builder.Configuration.GetSection("Spotify").Get<SpotifySettings>();
logger.LogInformation("Spotify Client ID: {ClientId}", spotifySettings?.ClientId ?? "Missing");
logger.LogInformation("Spotify Redirect URI: {RedirectUri}", spotifySettings?.RedirectUri ?? "Missing");

app.Run();
