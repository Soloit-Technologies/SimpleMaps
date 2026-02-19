using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using SimpleMaps.MapObjects.Geometries;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

public static class PointExtensions
{
    public static IFeature ToFeature(this Point point)
    {
        var feature = new PointFeature(point.Location.ToMPoint())
        {
            ["mapObject"] = point
        };

        feature.Styles.Add(new SymbolStyle
        {
            Fill = new Brush(point.Color.ToMapsuiColor()),
            SymbolType = SymbolType.Ellipse,
            SymbolScale = point.Size / 32.0
        });

        return feature;
    }
}
