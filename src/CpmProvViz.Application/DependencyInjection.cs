using CpmProvViz.Application.Graph.ReadGraph;
using Microsoft.Extensions.DependencyInjection;

namespace CpmProvViz.Application;

public static class DependencyInjection
{
    /// <summary>Registers the Application layer: its use cases.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IReadGraphUseCase, ReadGraphUseCase>();
        return services;
    }
}
