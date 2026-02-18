using System.ComponentModel;
using Mapsui;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.MapEngine.Implementations.Mapsui.Layers;
using SimpleMaps.MapEngine.Implementations.Mapsui.Tiles;
using SimpleMaps.MapObjects;
using SimpleMaps.Tiles;

namespace SimpleMaps.MapEngine.Implementations.Mapsui;

internal class MapsuiMapEngine : IMapEngine
{
    private readonly Map _map = new();

    private readonly MyLocationLayer _positionLayer;

    /// <summary>
    /// Layer name prefix for bottom system layers (e.g., base map)
    /// </summary>
    private const string BottomSystemLayerPrefix = "sys_bottom_";

    /// <summary>
    /// Layer name prefix for top system layers (e.g., position layer)
    /// </summary>
    private const string TopSystemLayerPrefix = "sys_top_";

    /// <summary>
    /// Layer name prefix for user layers
    /// </summary>
    private const string UserLayerPrefix = "user_";

    /// <summary>
    /// Layer name prefix for external tile layers provided via <see cref="ITileProvider"/>.
    /// </summary>
    private const string TileLayerPrefix = "tile_";

    /// <summary>
    /// Stores the desired visibility state for existing layers.
    /// Key is the layer index, value is the desired visibility state.
    /// This persists across layer replacements to ensure visibility is maintained.
    /// </summary>
    private readonly Dictionary<int, bool> _desiredVisibilityState = [];

    /// <summary>
    /// Stores the desired filter state for existing layers.
    /// Key is the layer index, value is the desired filter function.
    /// This persists across layer replacements to ensure filters are maintained.
    /// </summary>
    private readonly Dictionary<int, Func<MapObject, double, bool>> _desiredFilterState = [];

    /// <summary>
    /// Maps layer index to the underlying data provider for quick access.
    /// </summary>
    private readonly Dictionary<int, FilteredIndexedMemoryProvider> _layerProviders = [];

    /// <summary>
    /// Handler for ordinary layers (WritableLayer).
    /// </summary>
    private readonly ILayerHandler _ordinaryLayerHandler;

    /// <summary>
    /// Handler for performance layers (RasterizingTileLayer).
    /// </summary>
    private readonly ILayerHandler _performanceLayerHandler;

    public Map MapsuiMap => _map;

    public object NativeMap => _map;

    public event EventHandler<ViewportEventArgs>? ViewportChanged;

    public bool ShowLocationMarker
    { 
        get => _positionLayer.Enabled; 
        set => _positionLayer.Enabled = value; 
    }

    public Coordinate MyLocation => _positionLayer.MyLocation.ToCoordinate();

    public bool IsCentered
    {
        get => _positionLayer.IsCentered;
        set
        {
            _positionLayer.IsCentered = value;
            if (_positionLayer.Enabled && _positionLayer.IsCentered)
            {
                _map.Navigator.CenterOn(_positionLayer.MyLocation);
            }
        }
    }

    public MapsuiMapEngine()
    {
        DefaultRendererFactory.Create = () => new MapRenderer();

        _ordinaryLayerHandler = new OrdinaryLayerHandler(_map, features => SortFeatures(features));
        _performanceLayerHandler = new PerformanceLayerHandler(features => SortFeatures(features), _layerProviders);

        _positionLayer = new(_map)
        {
            Enabled = false,
            IsCentered = false,
            Name = $"{TopSystemLayerPrefix}position"
        };

        var mapLayer = OpenStreetMap.CreateTileLayer("simple_maps");
        mapLayer.Name = $"{BottomSystemLayerPrefix}baseMap";

        _map.Layers.Add(mapLayer);
        _map.Layers.Add(_positionLayer);

        _map.Navigator.ViewportChanged += OnNativeViewportChanged;
    }

    public void Add(MapObject mapObject, int zIndex = 0)
    {
        Add([mapObject], zIndex);
    }

    public void Add(IEnumerable<MapObject> mapObjects, int zIndex = 0)
    {  
        if (!mapObjects.Any())
        {
            return;
        }

        var features = mapObjects.Select(g => g.ToFeature());

        var existingFeatures = GetFeatures(zIndex);

        var allFeatures = existingFeatures.Concat(features).ToList();

        if (allFeatures.Count > 50)
        {
            ReplaceWithPerformanceLayer(allFeatures, zIndex);
            return;
        }

        Replace(allFeatures, zIndex);
    }

    public void Replace(IEnumerable<MapObject> mapObjects, int zIndex = 0)
    {
        if (!mapObjects.Any())
        {
            RemoveAll(zIndex);
            return;
        }

        var features = mapObjects.Select(g => g.ToFeature());

        if (features.Count() > 50)
        {
            ReplaceWithPerformanceLayer(features, zIndex);
            return;
        }

        Replace(features, zIndex);
    }

    private void Replace(IEnumerable<IFeature> features, int zIndex)
    {
        var newLayer = _ordinaryLayerHandler.CreateOrUpdateLayer(features, zIndex);
        ReplaceLayer(newLayer, zIndex);
    }

    private void ReplaceWithPerformanceLayer(IEnumerable<IFeature> features, int zIndex)
    {
        var newLayer = _performanceLayerHandler.CreateOrUpdateLayer(features, zIndex);
        ReplaceLayer(newLayer, zIndex);
    }

    private static IEnumerable<IFeature> SortFeatures(IEnumerable<IFeature> features)
    {
        return features.OrderBy(f => ((MapObject?)f["mapObject"])?.RenderingOrder);
    }

    private void ReplaceLayer(ILayer layer, int zIndex)
    {
        var layerName = $"{UserLayerPrefix}{zIndex}";
        var layerToBeRemoved = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        
        ApplyPendingVisibilityState(layer, zIndex, layerToBeRemoved);
        ApplyPendingFilterState(layer, zIndex);
        RemoveOldLayer(layerToBeRemoved, zIndex);
        InsertLayerAtCorrectPosition(layer, zIndex);
    }

    private void ApplyPendingVisibilityState(ILayer layer, int zIndex, ILayer? layerToBeRemoved)
    {
        if (_desiredVisibilityState.TryGetValue(zIndex, out var desiredVisibility))
        {
            layer.Enabled = desiredVisibility;
        }
        else
        {
            layer.Enabled = layerToBeRemoved?.Enabled ?? true;
        }
    }

    private void ApplyPendingFilterState(ILayer layer, int zIndex)
    {
        if (_desiredFilterState.TryGetValue(zIndex, out var desiredFilter))
        {
            _ordinaryLayerHandler.ApplyFilter(layer, desiredFilter);
            _performanceLayerHandler.ApplyFilter(layer, desiredFilter);
        }
    }

    private void RemoveOldLayer(ILayer? layerToBeRemoved, int zIndex)
    {
        if (layerToBeRemoved is not null)
        {
            _map.Layers.Remove(layerToBeRemoved);
            _layerProviders.Remove(zIndex);
        }
    }

    private void InsertLayerAtCorrectPosition(ILayer layer, int zIndex)
    {
        var insertPosition = CalculateInsertPosition(zIndex);
        _map.Layers.Insert(insertPosition, layer);
    }

    private int CalculateInsertPosition(int zIndex)
    {
        var insertPosition = 0;

        foreach (var existingLayer in _map.Layers)
        {
            if (IsBottomSystemLayer(existingLayer.Name))
            {
                insertPosition++;
            }
            else if (IsTileLayer(existingLayer.Name))
            {
                insertPosition++;
            }
            else if (IsUserLayer(existingLayer.Name) && HasLowerZIndex(existingLayer.Name, zIndex))
            {
                insertPosition++;
            }
        }

        return insertPosition;
    }

    private static bool HasLowerZIndex(string layerName, int zIndex)
    {
        return HasLowerZIndex(layerName, UserLayerPrefix, zIndex);
    }

    private IEnumerable<IFeature> GetFeatures(int layerIndex)
    {
        var layerName = $"{UserLayerPrefix}{layerIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        
        if (layer == null)
        {
            return [];
        }

        if (_ordinaryLayerHandler.CanHandle(layer))
        {
            return _ordinaryLayerHandler.GetFeatures(layer);
        }

        if (_performanceLayerHandler.CanHandle(layer))
        {
            return _performanceLayerHandler.GetFeatures(layer);
        }

        return [];
    }

    public void SetFilter(int layerIndex, Func<MapObject, double, bool> filter)
    {
        _desiredFilterState[layerIndex] = filter;
        
        var layerName = $"{UserLayerPrefix}{layerIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        
        if (layer is not null)
        {
            _ordinaryLayerHandler.ApplyFilter(layer, filter);
            _performanceLayerHandler.ApplyFilter(layer, filter);
        }
    }

    public void Refresh()
    {
        foreach (var layer in _map.Layers)
        {
            if (layer is RasterizingTileLayer rasterLayer)
            {
                rasterLayer.ClearCache();
            }
        }
    }

    public void SetVisible(int layerIndex, bool enable = true)
    {
        var layerName = $"{UserLayerPrefix}{layerIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        
        _desiredVisibilityState[layerIndex] = enable;
        
        if (layer is not null)
        {
            layer.Enabled = enable;
            _map.Refresh();
        }
    }

    public void RemoveAll(int zIndex = 0)
    {
        var layerName = $"{UserLayerPrefix}{zIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        if (layer is not null)
        {
            _map.Layers.Remove(layer);
            _desiredVisibilityState.Remove(zIndex);
            _desiredFilterState.Remove(zIndex);
            _layerProviders.Remove(zIndex);
        }
    }

    public void Remove(MapObject mapObject)
    {
        foreach (var layer in _map.Layers.ToList())
        {
            if (layer.Name?.StartsWith(UserLayerPrefix) != true)
            {
                continue;
            }

            RemoveFromLayer(layer, mapObject);
        }
    }

    private void RemoveFromLayer(ILayer layer, MapObject mapObject)
    {
        var zIndex = ExtractZIndexFromLayerName(layer.Name);
        if (zIndex is null)
        {
            return;
        }

        // Try both handlers to see which one can handle this layer
        var remainingFeatures = _ordinaryLayerHandler.TryRemoveAndGetRemaining(layer, mapObject);
        
        remainingFeatures ??= _performanceLayerHandler.TryRemoveAndGetRemaining(layer, mapObject);

        // If nothing was removed, exit
        if (remainingFeatures is null)
        {
            return;
        }

        // If no features remain, remove the layer entirely
        if (!remainingFeatures.Any())
        {
            RemoveLayer(layer, zIndex.Value);
        }
        else if (remainingFeatures.Count() <= 50)
        {
            // If we now have few enough features, use ordinary layer
            Replace(remainingFeatures, zIndex.Value);
        }
        else
        {
            // If we still have many features, use performance layer
            ReplaceWithPerformanceLayer(remainingFeatures, zIndex.Value);
        }
    }

    private static int? ExtractZIndexFromLayerName(string? layerName)
    {
        if (layerName is null)
        {
            return null;
        }

        var layerNameWithoutPrefix = layerName[UserLayerPrefix.Length..];
        return int.TryParse(layerNameWithoutPrefix, out var zIndex) ? zIndex : null;
    }

    private void RemoveLayer(ILayer layer, int zIndex)
    {
        _map.Layers.Remove(layer);
        _desiredVisibilityState.Remove(zIndex);
        _desiredFilterState.Remove(zIndex);
        _layerProviders.Remove(zIndex);
    }

    public void MoveLocationMarker(Coordinate location, double bearing)
    {
        var point = location.ToMPoint();

        _positionLayer.UpdateMyLocation(point);
        _positionLayer.UpdateMyDirection(bearing, _map.Navigator.Viewport.Rotation);
        
        if (IsCentered)
        {
            _map.Navigator.CenterOn(point, 400, Easing.QuarticOut);
        }
    }

    public void ZoomIn() => _map.Navigator.ZoomIn();

    public void ZoomOut() => _map.Navigator.ZoomOut();

    public void CenterOn(Coordinate location)
    {
        _map.Navigator.CenterOn(location.ToMPoint());
    }

    public void CenterOn(Coordinate location, double resolution)
    {
        _map.Navigator.CenterOnAndZoomTo(location.ToMPoint(), resolution);
    }

    public void FlyTo(Coordinate location)
    {
        _map.Navigator.FlyTo(location.ToMPoint(), _map.Navigator.Resolutions[15]);
    }

    public void ZoomAndCenterOn(IEnumerable<MapObject> mapObjects)
    {
        var extent = mapObjects.Select(mapObject => mapObject.ToFeature().Extent).Aggregate((extent1, extent2) => extent1?.Join(extent2));

        _map.Navigator.ZoomToBox(extent?.Grow(1000));
    }

    public void AddTileLayer(ITileProvider provider, int zIndex)
    {
        var layerName = $"{TileLayerPrefix}{zIndex}";

        var existing = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        if (existing is not null)
        {
            _map.Layers.Remove(existing);
        }

        var tileSource = new ExternalTileSource(provider);
        var tileLayer = new TileLayer(tileSource)
        {
            Name = layerName
        };

        var insertPosition = CalculateTileLayerInsertPosition(zIndex);
        _map.Layers.Insert(insertPosition, tileLayer);
    }

    public void RemoveTileLayer(int zIndex)
    {
        var layerName = $"{TileLayerPrefix}{zIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);

        if (layer is not null)
        {
            _map.Layers.Remove(layer);
        }
    }

    public void SetTileLayerVisible(int zIndex, bool visible = true)
    {
        var layerName = $"{TileLayerPrefix}{zIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);

        if (layer is not null)
        {
            layer.Enabled = visible;
            _map.Refresh();
        }
    }

    public void ClearTileLayerCache(int zIndex)
    {
        var layerName = $"{TileLayerPrefix}{zIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);

        if (layer is TileLayer tileLayer)
        {
            tileLayer.ClearCache();
            _map.Refresh();
        }
    }

    /// <summary>
    /// Calculates the correct insert position for a tile layer.
    /// Tile layers are inserted after bottom system layers and after any tile layers with lower z-index,
    /// but before user layers and top system layers.
    /// </summary>
    private int CalculateTileLayerInsertPosition(int zIndex)
    {
        var insertPosition = 0;

        foreach (var existingLayer in _map.Layers)
        {
            if (IsBottomSystemLayer(existingLayer.Name))
            {
                insertPosition++;
            }
            else if (IsTileLayer(existingLayer.Name) && HasLowerZIndex(existingLayer.Name, TileLayerPrefix, zIndex))
            {
                insertPosition++;
            }
        }

        return insertPosition;
    }

    private static bool HasLowerZIndex(string layerName, string prefix, int zIndex)
    {
        var layerNameWithoutPrefix = layerName[prefix.Length..];
        return int.TryParse(layerNameWithoutPrefix, out var existingIndex) && existingIndex < zIndex;
    }

    /// <summary>
    /// Checks if a layer name corresponds to a bottom system layer (rendered first).
    /// </summary>
    /// <param name="layerName">The layer name to check.</param>
    /// <returns>True if the layer is a bottom system layer; otherwise, false.</returns>
    private static bool IsBottomSystemLayer(string? layerName) => layerName?.StartsWith(BottomSystemLayerPrefix) == true;

    /// <summary>
    /// Checks if a layer name corresponds to a top system layer (rendered last).
    /// </summary>
    /// <param name="layerName">The layer name to check.</param>
    /// <returns>True if the layer is a top system layer; otherwise, false.</returns>
    private static bool IsTopSystemLayer(string? layerName) => layerName?.StartsWith(TopSystemLayerPrefix) == true;

    /// <summary>
    /// Checks if a layer name corresponds to a user layer.
    /// </summary>
    /// <param name="layerName">The layer name to check.</param>
    /// <returns>True if the layer is a user layer; otherwise, false.</returns>
    private static bool IsUserLayer(string? layerName) => layerName?.StartsWith(UserLayerPrefix) == true;

    /// <summary>
    /// Checks if a layer name corresponds to an external tile layer.
    /// </summary>
    /// <param name="layerName">The layer name to check.</param>
    /// <returns>True if the layer is a tile layer; otherwise, false.</returns>
    private static bool IsTileLayer(string? layerName) => layerName?.StartsWith(TileLayerPrefix) == true;

    private void OnNativeViewportChanged(object? sender, PropertyChangedEventArgs e)
    {
        var viewport = _map.Navigator.Viewport;
        var centerLonLat = SphericalMercator.ToLonLat(viewport.CenterX, viewport.CenterY);
        var center = new WGS84Coordinate(centerLonLat.lat, centerLonLat.lon);

        var extent = viewport.ToExtent();
        var tl = SphericalMercator.ToLonLat(extent.MinX, extent.MaxY);
        var tr = SphericalMercator.ToLonLat(extent.MaxX, extent.MaxY);
        var bl = SphericalMercator.ToLonLat(extent.MinX, extent.MinY);
        var br = SphericalMercator.ToLonLat(extent.MaxX, extent.MinY);

        ViewportChanged?.Invoke(this, new ViewportEventArgs(
            center,
            viewport.Resolution,
            new WGS84Coordinate(tl.lat, tl.lon),
            new WGS84Coordinate(tr.lat, tr.lon),
            new WGS84Coordinate(bl.lat, bl.lon),
            new WGS84Coordinate(br.lat, br.lon)));
    }
}
