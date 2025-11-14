using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using FestivalLineupBySpotify_API.Services;
using SpotifyAPI.Web;
using Spotify_Alonzzo_API.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://festivallineupbyspotify-fe.onrender.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
