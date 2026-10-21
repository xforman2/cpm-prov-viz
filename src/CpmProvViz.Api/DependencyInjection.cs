using System.Text.Json;
using System.Text.Json.Serialization;

namespace CpmProvViz.Api;

public static class DependencyInjection
{
    /// <summary>Registers the Api layer: problem details, JSON options and the OpenAPI document.</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        services.AddOpenApi();
        return services;
    }
}
