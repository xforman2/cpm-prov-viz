using CpmProvViz.Application.Ports;
using CpmProvViz.Infrastructure.ProvJson;
using Microsoft.Extensions.DependencyInjection;

namespace CpmProvViz.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the Infrastructure layer: the adapters behind the Application ports.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProvDocumentReader, ProvJsonReader>();
        return services;
    }
}
