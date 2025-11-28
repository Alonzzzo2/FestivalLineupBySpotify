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
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FestivalMatcher:";
});
builder.Services.AddScoped<ICacheService, DistributedCacheService>();

// Use in-memory session store for user-specific liked songs cache
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<GetFestivalsWithPlaylistRequest>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FestivalMatcherAPI.Swagger.AddAuthorizationHeaderForCacheRefreshOperationFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISpotifyClient, SpotifyClient>();
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
app.Run();
