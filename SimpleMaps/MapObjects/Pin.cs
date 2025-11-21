using SimpleMaps.Coordinates;
using System.Drawing;

namespace SimpleMaps.MapObjects;

public record Pin(Coordinate Location) : MapObject
{
    public Color Color { get; init; } = Color.Blue;
}
