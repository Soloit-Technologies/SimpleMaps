using Microsoft.Maui.Hosting;
using SimpleMaps.MapEngine;
using SimpleMaps.MapEngine.Implementations.Mapsui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SimpleMaps.Maui;

/// <summary>
/// Extension methods for configuring SimpleMaps in a .NET MAUI application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configures SimpleMaps services and components for a .NET MAUI application.
    /// Registers the IMapEngine service with its Mapsui implementation.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder to configure.</param>
    /// <returns>The MauiAppBuilder for chaining.</returns>
    public static MauiAppBuilder UseSimpleMaps(this MauiAppBuilder builder)
    {
        builder.UseSkiaSharp();
        builder.Services.AddSingleton<IMapEngine, MapsuiMapEngine>();
        return builder;
    }
}
