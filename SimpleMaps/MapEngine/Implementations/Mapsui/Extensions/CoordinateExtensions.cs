using Mapsui;
using Coordinate = SimpleMaps.Coordinates.Coordinate;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

public static class CoordinateExtensions
{
    public static MPoint ToMPoint(this Coordinate coordinate)
    {
        var webMeractor = coordinate.ToWebMercator();
        return new(webMeractor.X, webMeractor.Y);
    }

    public static NetTopologySuite.Geometries.Coordinate ToNTSCoordinate(this Coordinate coordinate)
    {
        var webMercator = coordinate.ToWebMercator();
        return new(webMercator.X, webMercator.Y);
    }
}
