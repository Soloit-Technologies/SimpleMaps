using SimpleMaps.Coordinates;
using System.Drawing;

namespace SimpleMaps.MapObjects.Geometries;

public record Point(Coordinate Location) : Geometry
{
    public Color Color { get; init; } = Color.Red;

    public int Size { get; init; } = 5;
}
