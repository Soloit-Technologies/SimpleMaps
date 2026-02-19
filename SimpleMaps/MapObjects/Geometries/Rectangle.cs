using SimpleMaps.Coordinates;

namespace SimpleMaps.MapObjects.Geometries;

/// <summary>
/// A rectangular polygon defined by four corner coordinates in WGS84.
/// </summary>
public record Rectangle : Polygon
{
    public WGS84Coordinate TopLeft { get; init; }
    public WGS84Coordinate TopRight { get; init; }
    public WGS84Coordinate BottomLeft { get; init; }
    public WGS84Coordinate BottomRight { get; init; }

    public Rectangle(WGS84Coordinate topLeft, WGS84Coordinate topRight,
                     WGS84Coordinate bottomLeft, WGS84Coordinate bottomRight)
        : base([topRight, topLeft, bottomLeft, bottomRight])
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    /// <summary>
    /// Creates a rectangle centered on a location with the given radius in meters.
    /// </summary>
    public static Rectangle FromLocation(WGS84Coordinate location, double radius)
    {
        var north = location.GetCoordinateAtDistance(radius, 0);
        var south = location.GetCoordinateAtDistance(radius, 180);
        var east = location.GetCoordinateAtDistance(radius, 90);
        var west = location.GetCoordinateAtDistance(radius, 270);

        var topLeft = new WGS84Coordinate(north.Latitude, west.Longitude);
        var topRight = new WGS84Coordinate(north.Latitude, east.Longitude);
        var bottomLeft = new WGS84Coordinate(south.Latitude, west.Longitude);
        var bottomRight = new WGS84Coordinate(south.Latitude, east.Longitude);

        return new Rectangle(topLeft, topRight, bottomLeft, bottomRight);
    }
}
