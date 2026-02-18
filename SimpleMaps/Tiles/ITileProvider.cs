namespace SimpleMaps.Tiles;

/// <summary>
/// Implement this interface to supply custom tile imagery for a map layer.
/// </summary>
public interface ITileProvider
{
    /// <summary>
    /// Returns the raw image bytes (PNG/JPEG) for the requested tile.
    /// Return an empty array to render nothing for this tile.
    /// </summary>
    /// <param name="tile">The tile metadata describing which tile is requested.</param>
    /// <returns>A byte array containing the tile image data.</returns>
    Task<byte[]> GetTileAsync(TileMetadata tile);
}
