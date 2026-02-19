using BruTile;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Tiling.Provider;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.MapObjects;

namespace SimpleMaps.Rendering;

/// <summary>
/// Renders a pre-loaded set of <see cref="MapObject"/>s into tile images.
/// Supports resolution-based visibility filtering and spatial querying.
/// </summary>
public class TileRenderer : IDisposable
{
    private readonly FilterableProvider _provider;
    private readonly RasterizingTileSource _tileSource;
    private bool _disposed;

    /// <summary>
    /// Creates a tile renderer from a collection of map objects.
    /// </summary>
    /// <param name="mapObjects">The map objects to render.</param>
    /// <param name="visibilityFilter">
    /// Optional filter controlling which objects are visible at a given resolution.
    /// Return true to show the object, false to hide it.
    /// </param>
    public TileRenderer(IEnumerable<MapObject> mapObjects, Func<MapObject, double, bool>? visibilityFilter = null)
    {
        DefaultRendererFactory.Create = () => new MapRenderer();

        var features = mapObjects.Select(m => m.ToFeature()).ToList();
        _provider = new FilterableProvider(features, visibilityFilter);

        Layer layer = new()
        {
            DataSource = _provider,
            Style = new VectorStyle
            {
                Fill = new Brush(Color.Transparent),
                Outline = new Pen(Color.Transparent)
            }
        };

        _tileSource = new RasterizingTileSource(layer);
    }

    /// <summary>
    /// Renders a tile at the given TMS coordinates.
    /// </summary>
    /// <param name="col">The tile column (X).</param>
    /// <param name="row">The tile row (Y).</param>
    /// <param name="level">The zoom level (Z).</param>
    /// <returns>The tile image as a byte array, or an empty array if no content.</returns>
    public async Task<byte[]> RenderTileAsync(int col, int row, int level)
    {
        var tileInfo = ToTileInfo(col, row, level);
        var tile = await _tileSource.GetTileAsync(tileInfo);
        return tile ?? [];
    }

    /// <summary>
    /// Queries map objects that intersect the given bounding box at the specified resolution.
    /// </summary>
    /// <param name="topLeft">Top-left corner in WGS84.</param>
    /// <param name="bottomRight">Bottom-right corner in WGS84.</param>
    /// <param name="resolution">The map resolution for visibility filtering.</param>
    /// <returns>The map objects that are visible and intersect the area.</returns>
    public async Task<IEnumerable<MapObject>> QueryAsync(
        WGS84Coordinate topLeft,
        WGS84Coordinate bottomRight,
        double resolution)
    {
        var (x, y) = SphericalMercator.FromLonLat(topLeft.Longitude, topLeft.Latitude);
        var br = SphericalMercator.FromLonLat(bottomRight.Longitude, bottomRight.Latitude);

        var extent = new MRect(
            Math.Min(x, br.x), Math.Min(y, br.y),
            Math.Max(x, br.x), Math.Max(y, br.y));

        var section = new MSection(extent, resolution);
        var fetchInfo = new FetchInfo(section);

        var features = await _provider.GetFeaturesAsync(fetchInfo);

        return features
            .Select(f => f["mapObject"] as MapObject)
            .Where(m => m is not null)
            .Cast<MapObject>();
    }

    /// <summary>
    /// Renders an ad-hoc collection of map objects into a single tile.
    /// Useful for dynamic content like measurement data that changes per request.
    /// </summary>
    public static async Task<byte[]> RenderOnceAsync(IEnumerable<MapObject> mapObjects, int col, int row, int level)
    {
        DefaultRendererFactory.Create = () => new MapRenderer();

        var features = mapObjects
            .OrderBy(m => m.RenderingOrder)
            .Select(m => m.ToFeature())
            .ToList();

        if (features.Count == 0)
        {
            features.Add(new PointFeature(0, 0));
        }

        using MemoryLayer layer = new()
        {
            Features = features,
            Style = new VectorStyle
            {
                Fill = new Brush(Color.Transparent),
                Outline = new Pen(Color.Transparent)
            }
        };

        RasterizingTileSource tileSource = new(layer);
        var tileInfo = ToTileInfo(col, row, level);
        var tile = await tileSource.GetTileAsync(tileInfo);
        return tile ?? [];
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    private static TileInfo ToTileInfo(int col, int row, int level)
    {
        double earthRadius = 6378137.0;
        double originShift = Math.PI * earthRadius;
        double initialResolution = 2 * Math.PI * earthRadius / 256;
        double resolution = initialResolution / Math.Pow(2, level);

        double maxy = originShift - col * 256 * resolution;
        double minx = row * 256 * resolution - originShift;
        double miny = originShift - (col + 1) * 256 * resolution;
        double maxx = (row + 1) * 256 * resolution - originShift;

        return new TileInfo
        {
            Index = new BruTile.TileIndex(row, col, level),
            Extent = new Extent(minx, miny, maxx, maxy)
        };
    }

    /// <summary>
    /// An IndexedMemoryProvider that supports resolution-based filtering and rendering order.
    /// </summary>
    private class FilterableProvider(
        IEnumerable<IFeature> features,
        Func<MapObject, double, bool>? visibilityFilter) : IndexedMemoryProvider(features)
    {
        public override async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var allFeatures = await base.GetFeaturesAsync(fetchInfo);

            var visibleFeatures = visibilityFilter is null
                ? allFeatures
                : allFeatures.Where(f =>
                    f["mapObject"] is MapObject mapObject &&
                    visibilityFilter(mapObject, fetchInfo.Resolution));

            return [.. visibleFeatures.OrderBy(f => f["mapObject"] is MapObject m ? m.RenderingOrder : 0)];
        }
    }
}
