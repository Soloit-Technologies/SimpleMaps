using Mapsui;
using SimpleMaps.MapObjects;
using SimpleMaps.MapObjects.Geometries;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class MapObjectExtensions
{
    public static IFeature ToFeature(this MapObject mapObject) => mapObject switch
    {
        Geometry geometry => geometry.ToFeature(),
        Label label => label.ToFeature(),
        Pin pin => pin.ToFeature(),
        _ => throw new NotSupportedException($"MapObject type '{mapObject.GetType().Name}' is not supported.")
    };
}
