using Mapsui;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Polygon = SimpleMaps.MapObjects.Geometries.Polygon;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class PolygonExtensions
{
    public static IFeature ToFeature(this Polygon polygon)
    {
        GeometryFeature feature = new()
        {
            Geometry = new NetTopologySuite.Geometries.Polygon(ToLinearRing(polygon.Vertices)),
            ["mapObject"] = polygon
        };

        feature.Styles.Add(GetStyle(polygon.Outline, polygon.Fill));

        return feature;
    }

    private static LinearRing ToLinearRing(IEnumerable<Coordinates.Coordinate> vertices)
    {
        var sphericals = vertices.Select(v => v.ToNTSCoordinate()).ToArray();

        if (sphericals.Length == 0)
        {
            return new([]);
        }
        else if (sphericals.Length == 1)
        {
            return new([sphericals[0], sphericals[0], sphericals[0]]);
        }

        return new([.. sphericals, sphericals[0]]);
    }

    private static VectorStyle GetStyle(MapObjects.Styles.Pen outline, System.Drawing.Color fill) => new()
    {
        Fill = new(fill.ToMapsuiColor()),
        Outline = new(outline.Color.ToMapsuiColor(), outline.Width)
        {
            StrokeJoin = StrokeJoin.Round,
            PenStrokeCap = PenStrokeCap.Round,
            PenStyle = outline.Stroke.ToPenStyle()
        },
        Line = null
    };
}
