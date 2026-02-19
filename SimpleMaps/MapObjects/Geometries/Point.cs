using SimpleMaps.Coordinates;
using System.Drawing;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;

namespace SimpleMaps.MapObjects.Geometries;

public record Point(Coordinate Location) : Geometry
{
    public Color Color { get; init; } = Color.Red;

    public int Size { get; init; } = 5;

    public override NetTopologySuite.Geometries.Geometry ToNtsGeometry()
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        var wgs84 = Location.ToWGS84();
        return factory.CreatePoint(new NtsCoordinate(wgs84.Longitude, wgs84.Latitude));
    }
}
