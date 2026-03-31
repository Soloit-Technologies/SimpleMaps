using MightyLittleGeodesy.Positions;

namespace SimpleMaps.Coordinates;

/// <summary>
/// Represents a geographic coordinate in the SWEREF99 TM (EPSG:3006) coordinate system.
/// Uses Northing (Y) and Easting (X) values in meters.
/// </summary>
/// <param name="northing">The northing value (Y) in meters.</param>
/// <param name="easting">The easting value (X) in meters.</param>
public class SWEREF99Coordinate(double northing, double easting) : Coordinate
{
    /// <summary>
    /// Gets the northing value (Y) in meters.
    /// </summary>
    public double Northing { get; } = northing;

    /// <summary>
    /// Gets the easting value (X) in meters.
    /// </summary>
    public double Easting { get; } = easting;

    public override CoordinateSystem CoordinateSystem => CoordinateSystem.SWEREF99;

    public override double X => Easting;

    public override double Y => Northing;

    public override Coordinate ConvertTo(CoordinateSystem targetSystem) => targetSystem switch
    {
        CoordinateSystem.SWEREF99 => new SWEREF99Coordinate(Northing, Easting),
        CoordinateSystem.WGS84 => ConvertToWGS84(),
        CoordinateSystem.RT90 => ConvertToWGS84().ConvertTo(CoordinateSystem.RT90),
        CoordinateSystem.WebMercator => ConvertToWGS84().ConvertTo(CoordinateSystem.WebMercator),
        _ => throw new ArgumentException($"Unknown coordinate system: {targetSystem}", nameof(targetSystem))
    };

    /// <summary>
    /// Creates a SWEREF99Coordinate from a WGS84 latitude and longitude.
    /// </summary>
    internal static SWEREF99Coordinate FromWGS84(double latitude, double longitude)
    {
        var wgs84 = new WGS84Position(latitude, longitude);
        var sweref = new SWEREF99Position(wgs84, SWEREF99Position.SWEREFProjection.sweref_99_tm);
        return new SWEREF99Coordinate(sweref.Latitude, sweref.Longitude);
    }

    private WGS84Coordinate ConvertToWGS84()
    {
        var sweref = new SWEREF99Position(Northing, Easting, SWEREF99Position.SWEREFProjection.sweref_99_tm);
        var wgs84 = sweref.ToWGS84();
        return new WGS84Coordinate(wgs84.Latitude, wgs84.Longitude);
    }

    public override string ToString() => $"SWEREF99({Northing:F0}, {Easting:F0})";

    public override bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        if (other is SWEREF99Coordinate sweref)
            return Math.Abs(Northing - sweref.Northing) < 0.01 && Math.Abs(Easting - sweref.Easting) < 0.01;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Northing, Easting);
}
