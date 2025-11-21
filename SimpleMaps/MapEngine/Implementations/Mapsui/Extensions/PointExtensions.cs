using Mapsui;
using Mapsui.Layers;
using SimpleMaps.MapObjects.Geometries;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class PointExtensions
{
    public static IFeature ToFeature(this Point point)
    {
        var feature = new PointFeature(point.Location.ToMPoint())
        {
            ["mapObject"] = point
        };

        return feature;
    }
}
