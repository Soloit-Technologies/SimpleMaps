using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects.Styles;

namespace SimpleMaps.MapObjects.Geometries;

public record LineString(IEnumerable<Coordinate> Vertices) : Geometry
{
    public Pen Stroke { get; init; } = new(System.Drawing.Color.Blue) { Width = 4 };
}
