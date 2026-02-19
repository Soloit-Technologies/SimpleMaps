using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleMaps.Coordinates;

/// <summary>
/// Represents a geographic coordinate in the WGS84 (World Geodetic System 1984) coordinate system.
/// Uses Latitude (Y) and Longitude (X) in degrees.
/// </summary>
public class WGS84Coordinate : Coordinate, IParsable<WGS84Coordinate>
{
    private const double MinLatitude = -90.0;
    private const double MaxLatitude = 90.0;
    private const double MinLongitude = -180.0;
    private const double MaxLongitude = 180.0;

    private double _latitude;
    private double _longitude;

    /// <summary>
    /// Initializes a new instance of the WGS84Coordinate class.
    /// </summary>
    [JsonConstructor]
    public WGS84Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Gets or sets the latitude value in degrees (-90 to 90).
    /// </summary>
    public double Latitude
    {
        get => _latitude;
        set
        {
            if (value < MinLatitude || value > MaxLatitude)
                throw new ArgumentOutOfRangeException(nameof(Latitude), $"Latitude must be between {MinLatitude} and {MaxLatitude}.");
            _latitude = value;
        }
    }

    /// <summary>
    /// Gets or sets the longitude value in degrees (-180 to 180).
    /// </summary>
    public double Longitude
    {
        get => _longitude;
        set
        {
            if (value < MinLongitude || value > MaxLongitude)
                throw new ArgumentOutOfRangeException(nameof(Longitude), $"Longitude must be between {MinLongitude} and {MaxLongitude}.");
            _longitude = value;
        }
    }

    public override CoordinateSystem CoordinateSystem => CoordinateSystem.WGS84;

    public override double X => Longitude;

    public override double Y => Latitude;

    public override Coordinate ConvertTo(CoordinateSystem targetSystem)
    {
        return targetSystem switch
        {
            CoordinateSystem.WGS84 => new WGS84Coordinate(Latitude, Longitude),
            CoordinateSystem.WebMercator => ConvertToWebMercator(),
            _ => throw new ArgumentException($"Unknown coordinate system: {targetSystem}", nameof(targetSystem))
        };
    }

    private WebMercatorCoordinate ConvertToWebMercator()
    {
        const double mercatorMax = 20037508.34;
        double x = Longitude * (mercatorMax / 180.0);
        double y = Math.Log(Math.Tan((90.0 + Latitude) * Math.PI / 360.0)) * (mercatorMax / Math.PI);
        return new WebMercatorCoordinate(x, y);
    }

    public double DistanceTo(WGS84Coordinate other)
    {
        const double earthRadiusMeters = 6371000.0;
        double lat1Rad = ToRadians(Latitude);
        double lat2Rad = ToRadians(other.Latitude);
        double deltaLatRad = ToRadians(other.Latitude - Latitude);
        double deltaLonRad = ToRadians(other.Longitude - Longitude);

        double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    public override WGS84Coordinate GetCoordinateAtDistance(double distance, double heading)
    {
        const double earthRadiusMeters = 6371000.0;
        double angularDistance = distance / earthRadiusMeters;
        double headingRad = ToRadians(heading);
        double lat1Rad = ToRadians(Latitude);
        double lon1Rad = ToRadians(Longitude);
        double lat2Rad = Math.Asin(Math.Sin(lat1Rad) * Math.Cos(angularDistance) +
                                   Math.Cos(lat1Rad) * Math.Sin(angularDistance) * Math.Cos(headingRad));
        double lon2Rad = lon1Rad + Math.Atan2(Math.Sin(headingRad) * Math.Sin(angularDistance) * Math.Cos(lat1Rad),
                                              Math.Cos(angularDistance) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad));
        return new WGS84Coordinate(ToDegrees(lat2Rad), ToDegrees(lon2Rad));
    }

    public override string ToString()
        => $"{Latitude.ToString(CultureInfo.InvariantCulture)},{Longitude.ToString(CultureInfo.InvariantCulture)}";

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public override bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        if (other is WGS84Coordinate wgs84)
            return Math.Abs(Latitude - wgs84.Latitude) < 1e-9 && Math.Abs(Longitude - wgs84.Longitude) < 1e-9;
        var otherWgs84 = other.ToWGS84();
        return Math.Abs(Latitude - otherWgs84.Latitude) < 1e-9 && Math.Abs(Longitude - otherWgs84.Longitude) < 1e-9;
    }

    public override bool Equals(object? obj) => obj is Coordinate coord && Equals(coord);

    public static bool operator ==(WGS84Coordinate? left, WGS84Coordinate? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(WGS84Coordinate? left, WGS84Coordinate? right)
        => !(left == right);

    public static WGS84Coordinate Parse(string s, IFormatProvider? provider)
    {
        var parts = s.Split(',');
        if (parts.Length != 2)
            throw new FormatException("Input string was not in a correct format.");

        if (double.TryParse(parts[0], NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var latitude) &&
            double.TryParse(parts[1], NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var longitude))
        {
            return new WGS84Coordinate(latitude, longitude);
        }

        throw new FormatException("Input string was not in a correct format.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out WGS84Coordinate result)
    {
        result = new(0, 0);
        if (string.IsNullOrEmpty(s)) return false;

        var parts = s.Split(',');
        if (parts.Length != 2) return false;

        if (double.TryParse(parts[0], NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var latitude) &&
            double.TryParse(parts[1], NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var longitude))
        {
            result = new WGS84Coordinate(latitude, longitude);
            return true;
        }

        return false;
    }
}
