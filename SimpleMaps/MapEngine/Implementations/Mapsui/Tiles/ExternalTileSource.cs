using BruTile;
using BruTile.Predefined;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Tiles;

/// <summary>
/// Adapts a SimpleMaps <see cref="SimpleMaps.Tiles.ITileProvider"/> to Mapsui's <see cref="ILocalTileSource"/>.
/// </summary>
internal class ExternalTileSource(SimpleMaps.Tiles.ITileProvider provider) : ILocalTileSource
{
    public ITileSchema Schema => new GlobalSphericalMercator();

    public string Name => string.Empty;

    public Attribution Attribution => new();

    public async Task<byte[]?> GetTileAsync(TileInfo tileInfo)
    {
        var metadata = tileInfo.ToTileMetadata();
        var data = await provider.GetTileAsync(metadata);
        return data.Length == 0 ? null : data;
    }
}
