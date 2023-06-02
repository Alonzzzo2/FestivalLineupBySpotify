﻿using SpotifyAPI.Web.Http;
using System.Text.RegularExpressions;

namespace FestivalLineupBySpotify_API.Services
{

    public static partial class SpotifyService
    {
        public static async Task<ClashFindersFavoritesResult> GenerateClashFindersFavoritesResult(HttpRequest request, string festivalName)
        {
            var spotifyClient = SpotifyApiService.CreateSpotifyClient(request.Cookies);
            var favArtists = await SpotifyApiService.GetFavoriteArtistsFromSpotify(spotifyClient);
            var festivalEvents = await ClashFindersService.GetEventsFromClashFinders(festivalName);

            var artistsWithEvents = GenerateArtistsWithEvents(favArtists, festivalEvents);
            var highlightsCollection = new ClashFindersService.HighlightsCollection();

            double groupSize = (double)artistsWithEvents.Count / (double)4;
            for (int i = 0; i < artistsWithEvents.Count; i++)
            {
                int normalizedScore = (int)(Math.Floor((double)(i / groupSize)));                
                var clashFindersShortArtistsNames = artistsWithEvents[i].Events
                    .Select(e => e.Short.Split('(')[0])
                    .Distinct()
                    .ToList();
                highlightsCollection[normalizedScore].ArtistsShortEventNames.AddRange(clashFindersShortArtistsNames);                                
            }

            var result = new ClashFindersFavoritesResult(highlightsCollection.GenerateUrl(festivalName), artistsWithEvents.Sum(a => a.NumOfLikedTracks));
            return result;
        }

        private static List<DTO.Artist> GenerateArtistsWithEvents(List<DTO.Artist> favArtists, List<DTO.Event> festivalEvents)
        {
            // for each fav artist, if he has a festival event, add that event to the artist's event
            favArtists.ForEach(favArtist =>
            {
                var favArtistNames = favArtist.Name.ToLower().Split(" ");
                var favArtistEvents = festivalEvents.Where(festivalEvent =>
                {
                    var festivalEventArtists = festivalEvent.Name.ToLower().Split(" ");
                    bool isFavArtistEvent = favArtistNames.All(favArtistName => festivalEventArtists.Contains(favArtistName));
                    return isFavArtistEvent;
                }).ToList();
                
                favArtist.Events.AddRange(favArtistEvents);
            });
            return favArtists.Where(favArtist => favArtist.Events.Any()).OrderByDescending(a => a.NumOfLikedTracks).ToList();
        }
    }
}