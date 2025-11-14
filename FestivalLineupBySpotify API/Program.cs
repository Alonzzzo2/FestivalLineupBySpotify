using Microsoft.AspNetCore.DataProtection;
using Spotify_Alonzzo_API.Services;
using FestivalLineupBySpotify_API.Configuration;
using FestivalLineupBySpotify_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure settings from appsettings
builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<ClashFindersSettings>(builder.Configuration.GetSection("ClashFinders"));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("Cors"));

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
    .SetApplicationName("FestivalLineupBySpotify")
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISpotifyApiService, SpotifyApiService>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddScoped<ClashFindersService>();
builder.Services.AddHttpClient<ClashFindersService>(client =>
{
    client.BaseAddress = new Uri("https://clashfinder.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

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

// Log all configuration at startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== FESTIVAL LINEUP BY SPOTIFY - STARTUP CONFIGURATION ===");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Port: {Port}", portValue ?? "Default");

var spotifySettings = builder.Configuration.GetSection("Spotify").Get<SpotifySettings>();
logger.LogInformation("--- SPOTIFY SETTINGS ---");
logger.LogInformation("ClientId: {ClientId}", string.IsNullOrEmpty(spotifySettings?.ClientId) ? "NOT SET" : "***SET***");
logger.LogInformation("RedirectUri: {RedirectUri}", spotifySettings?.RedirectUri ?? "NOT SET");

var clashFindersSettings = builder.Configuration.GetSection("ClashFinders").Get<ClashFindersSettings>();
logger.LogInformation("--- CLASHFINDERS SETTINGS ---");
logger.LogInformation("AuthUsername: {AuthUsername}", string.IsNullOrEmpty(clashFindersSettings?.AuthUsername) ? "NOT SET" : "***SET***");
logger.LogInformation("AuthPublicKey: {AuthPublicKey}", string.IsNullOrEmpty(clashFindersSettings?.AuthPublicKey) ? "NOT SET" : "***SET***");

logger.LogInformation("--- CORS SETTINGS ---");
if (corsSettings?.AllowedOrigins?.Length > 0)
{
    logger.LogInformation("AllowedOrigins: {AllowedOrigins}", string.Join(", ", corsSettings.AllowedOrigins));
}
else
{
    logger.LogWarning("AllowedOrigins: EMPTY - CORS will not allow any origins!");
}

logger.LogInformation("=== END STARTUP CONFIGURATION ===");

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
app.Run();
