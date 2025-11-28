using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FestivalMatcherAPI.Swagger
{
    public class AddAuthorizationHeaderForCacheRefreshOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionDescriptor = context.ApiDescription.ActionDescriptor;
            if (actionDescriptor.RouteValues["controller"] == "Cache" &&
                actionDescriptor.RouteValues["action"] == "RefreshCache")
            {
                operation.Parameters ??= new List<OpenApiParameter>();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Bearer {RefreshKey}",
                    Schema = new OpenApiSchema { Type = "string" }
                });
            }
        }
    }
}
