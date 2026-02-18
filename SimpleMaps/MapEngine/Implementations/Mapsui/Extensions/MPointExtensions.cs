using Mapsui;
using SimpleMaps.Coordinates;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

public static class MPointExtensions
{
    public static Coordinate ToCoordinate(this MPoint point) => new WebMercatorCoordinate(point.X, point.Y);
}
