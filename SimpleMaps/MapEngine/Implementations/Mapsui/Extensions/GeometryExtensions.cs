using Mapsui;
using SimpleMaps.MapObjects.Geometries;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class GeometryExtensions
{
    public static IFeature ToFeature(this Geometry geometry)
    {
        return geometry switch
        {
            Point point => point.ToFeature(),
            _ => throw new NotSupportedException($"Geometry type '{geometry.GetType().Name}' is not supported.")
        };
    }
}
