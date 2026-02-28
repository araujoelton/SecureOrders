using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecureOrders.Api.Swagger;

public sealed class AllowAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
            || context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>().Any() == true;

        if (hasAllowAnonymous)
            operation.Security?.Clear();
    }
}
