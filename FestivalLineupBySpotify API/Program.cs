using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using FestivalLineupBySpotify_API.Services;
using SpotifyAPI.Web;
using Spotify_Alonzzo_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();

var app = builder.Build();
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
