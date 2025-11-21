using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects.Styles;
using System.Drawing;

namespace SimpleMaps.MapObjects.Geometries;

public record Polygon(IEnumerable<Coordinate> Vertices) : Geometry
{
    public Color Fill { get; init; } = Color.Transparent;

    public Pen Outline { get; init; } = new(Color.Black);
}
