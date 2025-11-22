using SpotifyAPI.Web;
using System.Text.Json;
using Spotify_Alonzzo_API.Controllers.DTO;

namespace FestivalLineupBySpotify_API.Services
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ErrorResponse response;

            if (exception is APIException apiEx)
            {
                context.Response.StatusCode = (int)(apiEx.Response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                response = new ErrorResponse(
                    context.Response.StatusCode,
                    "Spotify API error",
                    apiEx.Response?.Body?.ToString() ?? apiEx.Message
                );
            }
            else if (exception is InvalidOperationException)
            {
                context.Response.StatusCode = 400;
                response = new ErrorResponse(400, "Invalid operation", exception.Message);
            }
            else
            {
                context.Response.StatusCode = 500;
                response = new ErrorResponse(500, "Internal server error", exception.Message);
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
