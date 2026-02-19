using System.Text.Json.Serialization;

namespace SimpleMaps.Coordinates;

/// <summary>
/// Abstract base class representing a geographic coordinate in a specific coordinate system.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "coordinateSystem")]
[JsonDerivedType(typeof(WGS84Coordinate), "WGS84")]
[JsonDerivedType(typeof(WebMercatorCoordinate), "WebMercator")]
[JsonDerivedType(typeof(RT90Coordinate), "RT90")]
public abstract class Coordinate : IEquatable<Coordinate>
{
    /// <summary>
    /// Gets the coordinate system type for this coordinate.
    /// </summary>
    public abstract CoordinateSystem CoordinateSystem { get; }

    /// <summary>
    /// Gets the X value (typically Longitude or Easting depending on the coordinate system).
    /// </summary>
    public abstract double X { get; }

    /// <summary>
    /// Gets the Y value (typically Latitude or Northing depending on the coordinate system).
    /// </summary>
    public abstract double Y { get; }

    /// <summary>
    /// Converts this coordinate to a different coordinate system.
    /// </summary>
    /// <param name="targetSystem">The target coordinate system to convert to.</param>
    /// <returns>A new Coordinate instance in the target coordinate system.</returns>
    public abstract Coordinate ConvertTo(CoordinateSystem targetSystem);

    /// <summary>
    /// Converts this coordinate to WGS84 (Latitude/Longitude).
    /// </summary>
    /// <returns>A WGS84Coordinate instance.</returns>
    public WGS84Coordinate ToWGS84()
    {
        if (this is WGS84Coordinate wgs84)
            return wgs84;

        return (WGS84Coordinate)ConvertTo(CoordinateSystem.WGS84);
    }

    /// <summary>
    /// Converts this coordinate to Web Mercator projection (EPSG:3857).
    /// </summary>
    /// <returns>A WebMercatorCoordinate instance.</returns>
    public WebMercatorCoordinate ToWebMercator()
    {
        if (this is WebMercatorCoordinate webMercator)
            return webMercator;

        return (WebMercatorCoordinate)ConvertTo(CoordinateSystem.WebMercator);
    }

    /// <summary>
    /// Converts this coordinate to RT90 2.5 gon V (Swedish national grid).
    /// </summary>
    /// <returns>An RT90Coordinate instance.</returns>
    public RT90Coordinate ToRT90()
    {
        if (this is RT90Coordinate rt90)
            return rt90;

        return (RT90Coordinate)ConvertTo(CoordinateSystem.RT90);
    }

    /// <summary>
    /// Calculates the great-circle distance between this coordinate and another coordinate.
    /// </summary>
    /// <param name="other">The other coordinate to calculate distance to.</param>
    /// <returns>Distance in meters.</returns>
    public double DistanceTo(Coordinate other)
    {
        var thisWgs84 = ToWGS84();
        var otherWgs84 = other.ToWGS84();

        return thisWgs84.DistanceTo(otherWgs84);
    }

    /// <summary>
    /// Calculates a new coordinate at a given distance and heading from this coordinate.
    /// </summary>
    /// <remarks>
    /// This method uses the haversine formula to calculate the destination point on the Earth's surface
    /// given a starting point, distance, and bearing (heading). The calculation is performed in WGS84
    /// (Latitude/Longitude) coordinate system and then converted back to the original coordinate system if needed.
    ///
    /// For Web Mercator and other projected coordinate systems, the conversion to WGS84 is performed first,
    /// the calculation is done, and then the result remains in WGS84. If you need the result in a different
    /// coordinate system, use ConvertTo().
    /// </remarks>
    /// <param name="distance">The distance in meters from this coordinate to the target coordinate.</param>
    /// <param name="heading">The bearing/heading in degrees (0-360) from north.
    /// 0° = North, 90° = East, 180° = South, 270° = West.</param>
    /// <returns>A new Coordinate instance representing the destination point.
    /// Returns a WGS84Coordinate when called on non-WGS84 coordinate types.</returns>
    /// <example>
    /// <code>
    /// var startPoint = new WGS84Coordinate(51.5074, -0.1278); // London
    /// var distance = 1000; // 1 kilometer
    /// var heading = 45; // Northeast
    /// var destination = startPoint.GetCoordinateAtDistance(distance, heading);
    /// // destination will be approximately 1km northeast of London
    /// </code>
    /// </example>
    public virtual Coordinate GetCoordinateAtDistance(double distance, double heading)
    {
        return ToWGS84().GetCoordinateAtDistance(distance, heading);
    }

    protected static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
    protected static double ToDegrees(double radians) => radians * (180.0 / Math.PI);

    public abstract bool Equals(Coordinate? other);

    public override bool Equals(object? obj) => Equals(obj as Coordinate);

    public abstract override int GetHashCode();
}
