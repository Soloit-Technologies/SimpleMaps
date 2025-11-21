using Mapsui;
using SimpleMaps.Coordinates;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class MPointExtensions
{
    public static Coordinate ToCoordinate(this MPoint point) => new WebMercatorCoordinate(point.X, point.Y);
}
