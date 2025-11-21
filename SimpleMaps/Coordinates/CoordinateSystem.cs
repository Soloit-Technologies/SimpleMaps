namespace SimpleMaps.Coordinates
{
    /// <summary>
    /// Enumeration of supported coordinate systems.
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// World Geodetic System 1984 - Latitude/Longitude (degrees)
        /// </summary>
        WGS84 = 0,

        /// <summary>
        /// Web Mercator (EPSG:3857) - commonly used for web mapping applications (meters)
        /// </summary>
        WebMercator = 2,
    }
}
