namespace SimpleMaps.MapObjects.Geometries;

public abstract record Geometry : MapObject
{
    /// <summary>
    /// Converts this geometry to a NetTopologySuite geometry in WGS84 (SRID 4326).
    /// </summary>
    public abstract NetTopologySuite.Geometries.Geometry ToNtsGeometry();

    /// <summary>
    /// Tests whether this geometry spatially intersects another geometry.
    /// </summary>
    public bool Intersects(Geometry other) => ToNtsGeometry().Intersects(other.ToNtsGeometry());
}
