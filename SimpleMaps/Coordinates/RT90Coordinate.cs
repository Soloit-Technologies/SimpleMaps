using MightyLittleGeodesy.Positions;

namespace SimpleMaps.Coordinates;

/// <summary>
/// Represents a geographic coordinate in the RT90 2.5 gon V (Swedish national grid) coordinate system.
/// Uses Northing (Y) and Easting (X) values in meters.
/// </summary>
/// <remarks>
/// Initializes a new instance of the RT90Coordinate class.
/// </remarks>
/// <param name="northing">The northing value (Y) in meters.</param>
/// <param name="easting">The easting value (X) in meters.</param>
public class RT90Coordinate(double northing, double easting) : Coordinate
{

    /// <summary>
    /// Gets the northing value (Y) in meters.
    /// </summary>
    public double Northing { get; } = northing;

    /// <summary>
    /// Gets the easting value (X) in meters.
    /// </summary>
    public double Easting { get; } = easting;

    public override CoordinateSystem CoordinateSystem => CoordinateSystem.RT90;

    public override double X => Easting;

    public override double Y => Northing;

    public override Coordinate ConvertTo(CoordinateSystem targetSystem) => targetSystem switch
    {
        CoordinateSystem.RT90 => new RT90Coordinate(Northing, Easting),
        CoordinateSystem.WGS84 => ConvertToWGS84(),
        CoordinateSystem.WebMercator => ConvertToWGS84().ConvertTo(CoordinateSystem.WebMercator),
        _ => throw new ArgumentException($"Unknown coordinate system: {targetSystem}", nameof(targetSystem))
    };

    /// <summary>
    /// Creates an RT90Coordinate from a WGS84 latitude and longitude.
    /// </summary>
    internal static RT90Coordinate FromWGS84(double latitude, double longitude)
    {
        var wgs84 = new WGS84Position(latitude, longitude);
        var rt90 = new RT90Position(wgs84, RT90Position.RT90Projection.rt90_2_5_gon_v);
        return new RT90Coordinate(rt90.Latitude, rt90.Longitude);
    }

    private WGS84Coordinate ConvertToWGS84()
    {
        var rt90Position = new RT90Position(Northing, Easting, RT90Position.RT90Projection.rt90_2_5_gon_v);
        var wgs84 = rt90Position.ToWGS84();
        return new WGS84Coordinate(wgs84.Latitude, wgs84.Longitude);
    }

    public override string ToString() => $"RT90({Northing:F0}, {Easting:F0})";

    public override bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        if (other is RT90Coordinate rt90)
            return Math.Abs(Northing - rt90.Northing) < 0.01 && Math.Abs(Easting - rt90.Easting) < 0.01;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Northing, Easting);
}
