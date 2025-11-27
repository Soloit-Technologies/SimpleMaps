using Mapsui;
using Mapsui.Nts;
using Mapsui.Styles;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class LineStringExtensions
{
    public static IFeature ToFeature(this MapObjects.Geometries.LineString lineString)
    {
        GeometryFeature feature = new()
        {
            Geometry = new NetTopologySuite.Geometries.LineString([.. lineString.Vertices.Select(v => v.ToNTSCoordinate())]),
            ["mapObject"] = lineString
        };

        feature.Styles.Add(GetStyle(lineString.Stroke));
        return feature;
    }

    private static VectorStyle GetStyle(MapObjects.Styles.Pen pen) => new()
    {
        Fill = null,
        Outline = null,
        Line = new Pen(pen.Color.ToMapsuiColor(), pen.Width)
        {
            StrokeJoin = StrokeJoin.Round,
            PenStrokeCap = PenStrokeCap.Round,
            PenStyle = pen.Stroke.ToPenStyle()
        }
    };
}
