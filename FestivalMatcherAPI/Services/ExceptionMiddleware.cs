using SpotifyAPI.Web;
using System.Text.Json;
using FestivalMatcherAPI.Controllers.DTO;

namespace FestivalMatcherAPI.Services
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ErrorResponse response;

            if (exception is APIException apiEx)
            {
                _logger.LogError(exception, "Spotify API Error: {Message}", apiEx.Message);
                context.Response.StatusCode = (int)(apiEx.Response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                response = new ErrorResponse(
                    context.Response.StatusCode,
                    "Spotify API error",
                    apiEx.Response?.Body?.ToString() ?? apiEx.Message
                );
            }
            else if (exception is InvalidOperationException)
            {
                _logger.LogWarning(exception, "Invalid Operation: {Message}", exception.Message);
                context.Response.StatusCode = 400;
                response = new ErrorResponse(400, "Invalid operation", exception.Message);
            }
            else
            {
                _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
                context.Response.StatusCode = 500;
                response = new ErrorResponse(500, "Internal server error", exception.Message);
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
