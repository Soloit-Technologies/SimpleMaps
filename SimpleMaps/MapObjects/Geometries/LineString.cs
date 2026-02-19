using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects.Styles;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;

namespace SimpleMaps.MapObjects.Geometries;

public record LineString(IEnumerable<Coordinate> Vertices) : Geometry
{
    public Pen Stroke { get; init; } = new(System.Drawing.Color.Blue) { Width = 4 };

    public override NetTopologySuite.Geometries.Geometry ToNtsGeometry()
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        var coords = Vertices.Select(v =>
        {
            var wgs84 = v.ToWGS84();
            return new NtsCoordinate(wgs84.Longitude, wgs84.Latitude);
        }).ToArray();

        return factory.CreateLineString(coords);
    }
}
