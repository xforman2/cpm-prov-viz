using CpmProvViz.Application.Graph.ReadGraph;
using CpmProvViz.Domain.Cpm;
using Microsoft.Extensions.DependencyInjection;

namespace CpmProvViz.Application;

public static class DependencyInjection
{
    /// <summary>Registers the Application layer: its use cases and the services they use.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IReadGraphUseCase, ReadGraphUseCase>();
        services.AddSingleton<ICpmInterpreter, CpmInterpreter>();
        return services;
    }
}
