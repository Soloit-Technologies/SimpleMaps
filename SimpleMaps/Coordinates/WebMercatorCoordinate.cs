namespace SimpleMaps.Coordinates;

/// <summary>
/// Represents a geographic coordinate in Web Mercator projection (EPSG:3857).
/// Uses X (easting) and Y (northing) values in meters.
/// </summary>
/// <remarks>
/// Initializes a new instance of the WebMercatorCoordinate class.
/// </remarks>
/// <param name="x">The X value (easting) in meters.</param>
/// <param name="y">The Y value (northing) in meters.</param>
public class WebMercatorCoordinate(double x, double y) : Coordinate
{
    private const double MercatorMax = 20037508.34;

    public override CoordinateSystem CoordinateSystem => CoordinateSystem.WebMercator;

    public override double X => x;

    public override double Y => y;

    public override Coordinate ConvertTo(CoordinateSystem targetSystem) => targetSystem switch
    {
        CoordinateSystem.WebMercator => new WebMercatorCoordinate(X, Y),
        CoordinateSystem.WGS84 => ConvertToWGS84(),
        CoordinateSystem.RT90 => ConvertToWGS84().ConvertTo(CoordinateSystem.RT90),
        _ => throw new ArgumentException($"Unknown coordinate system: {targetSystem}", nameof(targetSystem))
    };

    /// <summary>
    /// Converts this Web Mercator coordinate to WGS84.
    /// </summary>
    private WGS84Coordinate ConvertToWGS84()
    {
        double longitude = (X / MercatorMax) * 180.0;
        double latitude = (Math.Atan(Math.Exp((Y / MercatorMax) * Math.PI)) * 2 - Math.PI / 2) * (180.0 / Math.PI);
        return new WGS84Coordinate(latitude, longitude);
    }

    public override string ToString() => $"WebMercator({X:F2}, {Y:F2})";

    public override bool Equals(Coordinate? other)
    {
        if (other is not WebMercatorCoordinate)
        {
            return false;
        }

        return Math.Abs(X - other.X) < 0.01 && Math.Abs(Y - other.Y) < 0.01;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}
