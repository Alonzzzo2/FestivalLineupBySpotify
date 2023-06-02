using SpotifyAPI.Web;

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
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                if (ex is APIException)
                {
                    var apiException = ex as APIException;
                    await context.Response.WriteAsync("An error occurred: " + apiException.Message + ";\r\n " + apiException.Response.Body);
                }
                else
                {
                    await context.Response.WriteAsync("An error occurred: " + ex.Message);
                }
            }
        }

    }
}
