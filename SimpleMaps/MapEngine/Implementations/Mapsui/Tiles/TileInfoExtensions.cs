using BruTile;
using Mapsui;
using Mapsui.Tiling.Extensions;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.Tiles;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Tiles;

internal static class TileInfoExtensions
{
    public static TileMetadata ToTileMetadata(this TileInfo tileInfo)
    {
        var rect = tileInfo.Extent.ToMRect();

        var topLeft = rect.GetTopLeft().ToCoordinate().ToWGS84();
        var topRight = rect.GetTopRight().ToCoordinate().ToWGS84();
        var bottomLeft = rect.GetBottomLeft().ToCoordinate().ToWGS84();
        var bottomRight = rect.GetBottomRight().ToCoordinate().ToWGS84();

        return new TileMetadata
        {
            Level = tileInfo.Index.Level,
            Col = tileInfo.Index.Col,
            Row = tileInfo.Index.Row,
            TopLeft = topLeft,
            TopRight = topRight,
            BottomLeft = bottomLeft,
            BottomRight = bottomRight,
        };
    }
}
