using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects.Styles;
using System.Drawing;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;

namespace SimpleMaps.MapObjects.Geometries;

public record Polygon(IEnumerable<Coordinate> Vertices) : Geometry
{
    public Color Fill { get; init; } = Color.Transparent;

    public Pen Outline { get; init; } = new(Color.Black);

    public override NetTopologySuite.Geometries.Geometry ToNtsGeometry()
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        var coords = Vertices.Select(v =>
        {
            var wgs84 = v.ToWGS84();
            return new NtsCoordinate(wgs84.Longitude, wgs84.Latitude);
        }).ToArray();

        if (coords.Length == 0)
            return factory.CreatePolygon();

        if (!coords[^1].Equals2D(coords[0]))
            coords = [.. coords, coords[0]];

        var polygon = factory.CreatePolygon(coords);

        if (!polygon.Shell.IsCCW)
            polygon = (NetTopologySuite.Geometries.Polygon)polygon.Reverse();

        return polygon;
    }
}
