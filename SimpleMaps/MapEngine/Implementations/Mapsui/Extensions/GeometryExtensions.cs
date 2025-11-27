using Mapsui;
using SimpleMaps.MapObjects.Geometries;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class GeometryExtensions
{
    public static IFeature ToFeature(this Geometry geometry) => geometry switch
    {
        Point point => point.ToFeature(),
        Polygon polygon => polygon.ToFeature(),
        LineString lineString => lineString.ToFeature(),
        _ => throw new NotSupportedException($"Geometry type '{geometry.GetType().Name}' is not supported.")
    };
}
