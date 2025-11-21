using Microsoft.Extensions.DependencyInjection;
using SimpleMaps.MapEngine;
using SimpleMaps.MapEngine.Implementations.Mapsui;

namespace SimpleMaps;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the IMapEngine service and its Mapsui implementation in the dependency injection container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the service to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddSimpleMaps(this IServiceCollection services)
    {
        services.AddSingleton<IMapEngine, MapsuiMapEngine>();
        return services;
    }
}
