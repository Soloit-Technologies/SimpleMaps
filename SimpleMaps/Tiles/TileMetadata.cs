using SimpleMaps.Coordinates;

namespace SimpleMaps.Tiles;

/// <summary>
/// Describes a single map tile's position and geographic extent.
/// </summary>
public class TileMetadata
{
    /// <summary>
    /// The tile column index.
    /// </summary>
    public int Col { get; init; }

    /// <summary>
    /// The tile row index.
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// The zoom level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// The top-left corner of the tile in WGS84.
    /// </summary>
    public required WGS84Coordinate TopLeft { get; init; }

    /// <summary>
    /// The top-right corner of the tile in WGS84.
    /// </summary>
    public required WGS84Coordinate TopRight { get; init; }

    /// <summary>
    /// The bottom-left corner of the tile in WGS84.
    /// </summary>
    public required WGS84Coordinate BottomLeft { get; init; }

    /// <summary>
    /// The bottom-right corner of the tile in WGS84.
    /// </summary>
    public required WGS84Coordinate BottomRight { get; init; }
}
