using Mapsui;
using Mapsui.Animations;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.MapObjects;

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
    /// Stores pending visibility states for layers that don't exist yet.
    /// Key is the layer index, value is the desired visibility state.
    /// </summary>
    private readonly Dictionary<int, bool> _pendingVisibilityState = new();

    /// <summary>
    /// Stores pending filter states for layers that don't exist yet.
    /// Key is the layer index, value is the desired filter function.
    /// </summary>
    private readonly Dictionary<int, Func<MapObject, double, bool>> _pendingFilterState = new();

    /// <summary>
    /// Maps layer index to the underlying data provider for quick access.
    /// </summary>
    private readonly Dictionary<int, FilteredIndexedMemoryProvider> _layerProviders = new();

    public Map MapsuiMap => _map;

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
    }

    public void Add(MapObject mapObject, int zIndex = 0)
    {
        Add([mapObject], zIndex);
    }

    public void Add(IEnumerable<MapObject> mapObjects, int zIndex = 0)
    {
        var features = mapObjects.Select(g => g.ToFeature());

        var existingFeatures = GetFeatures(zIndex);

        Replace(existingFeatures.Concat(features), zIndex);
    }

    public void Replace(IEnumerable<MapObject> mapObjects, int zIndex = 0)
    {
        var features = mapObjects.Select(g => g.ToFeature());

        Replace(features, zIndex);
    }

    private void Replace(IEnumerable<IFeature> features, int zIndex)
    {
        var provider = new FilteredIndexedMemoryProvider(features)
        {
            Sort = SortFeatures
        };

        Layer layer = new()
        {
            DataSource = provider,
            Style = new VectorStyle()
            {
                Fill = new(Color.Transparent),
                Outline = new(Color.Transparent)
            }
        };

        RasterizingTileLayer tileLayer = new(layer)
        {
            Name = $"{UserLayerPrefix}{zIndex}",
        };

        // Store the provider for later access
        _layerProviders[zIndex] = provider;

        ReplaceLayer(tileLayer, zIndex);
    }

    private static IEnumerable<IFeature> SortFeatures(IEnumerable<IFeature> features)
    {
        return features.OrderBy(f => ((MapObject?)f["mapObject"])?.RenderingOrder);
    }

    private void ReplaceLayer(RasterizingTileLayer layer, int zIndex)
    {
        var layerName = $"{UserLayerPrefix}{zIndex}";
        var layerToBeRemoved = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        
        // Apply pending visibility state if it exists, otherwise preserve existing state
        if (_pendingVisibilityState.TryGetValue(zIndex, out var pendingVisibility))
        {
            layer.Enabled = pendingVisibility;
            _pendingVisibilityState.Remove(zIndex);
        }
        else
        {
            layer.Enabled = layerToBeRemoved?.Enabled ?? true;
        }

        // Apply pending filter state if it exists
        if (_pendingFilterState.TryGetValue(zIndex, out var pendingFilter))
        {
            if (_layerProviders.TryGetValue(zIndex, out var provider))
            {
                provider.Filter = (feature, resolution) =>
                {
                    var mapObject = (MapObject?)feature["mapObject"];
                    if (mapObject is null)
                    {
                        return false;
                    }

                    return pendingFilter(mapObject, resolution);
                };
            }
            _pendingFilterState.Remove(zIndex);
        }

        if (layerToBeRemoved is not null)
        {
            _map.Layers.Remove(layerToBeRemoved);
            _layerProviders.Remove(zIndex);
        }
        
        // Find the correct position to insert based on logical layer indices
        // Layer order should be: bottom system layers -> user layers (by z-index) -> top system layers
        var insertPosition = 0;
        foreach (var existingLayer in _map.Layers)
        {
            // Count bottom system layers (they stay first)
            if (IsBottomSystemLayer(existingLayer.Name))
            {
                insertPosition++;
                continue;
            }

            // Count user layers with lower z-index
            if (IsUserLayer(existingLayer.Name))
            {
                var layerNameWithoutPrefix = existingLayer.Name[UserLayerPrefix.Length..];
                if (int.TryParse(layerNameWithoutPrefix, out var existingIndex) && existingIndex < zIndex)
                {
                    insertPosition++;
                }
            }
        }
        
        _map.Layers.Insert(insertPosition, layer);
    }

    private IReadOnlyList<IFeature> GetFeatures(int layerIndex)
    {
        var layerName = $"{UserLayerPrefix}{layerIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        if (layer == null)
        {
            return [];
        }

        var features = ((IndexedMemoryProvider?)((Layer)layer).DataSource)!.Features;
        
        return features;
    }

    public void SetFilter(int layerIndex, Func<MapObject, double, bool> filter)
    {
        if (_layerProviders.TryGetValue(layerIndex, out var provider))
        {
            provider.Filter = (feature, resolution) =>
            {
                var mapObject = (MapObject?)feature["mapObject"];
                if (mapObject is null)
                {
                    return false;
                }

                return filter(mapObject, resolution);
            };
            _pendingFilterState.Remove(layerIndex);
        }
        else
        {
            // Store the desired filter state for when the layer is created
            _pendingFilterState[layerIndex] = filter;
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
        
        if (layer is not null)
        {
            layer.Enabled = enable;
            _map.Refresh();

            // Remove from pending state if it was there
            _pendingVisibilityState.Remove(layerIndex);
        }
        else
        {
            // Store the desired visibility state for when the layer is created
            _pendingVisibilityState[layerIndex] = enable;
        }
    }

    public void RemoveAll(int zIndex = 0)
    {
        var layerName = $"{UserLayerPrefix}{zIndex}";
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerName);
        if (layer is not null)
        {
            _map.Layers.Remove(layer);
            _pendingVisibilityState.Remove(zIndex);
            _pendingFilterState.Remove(zIndex);
            _layerProviders.Remove(zIndex);
        }
    }

    public void Remove(MapObject mapObject)
    {
        foreach (var layer in _map.Layers.ToList())
        {
            if (layer.Name?.StartsWith(UserLayerPrefix) == true)
            {
                if (((Layer)layer).DataSource is IndexedMemoryProvider provider)
                {
                    var remainingFeatures = provider.Features
                        .Where(f => !Equals(f["mapObject"], mapObject))
                        .ToList();

                    if (remainingFeatures.Count != provider.Features.Count)
                    {
                        // Extract the z-index from the layer name
                        var layerNameWithoutPrefix = layer.Name[UserLayerPrefix.Length..];
                        if (int.TryParse(layerNameWithoutPrefix, out var zIndex))
                        {
                            if (remainingFeatures.Count == 0)
                            {
                                _map.Layers.Remove(layer);
                                _pendingVisibilityState.Remove(zIndex);
                                _pendingFilterState.Remove(zIndex);
                                _layerProviders.Remove(zIndex);
                            }
                            else
                            {
                                Replace(remainingFeatures, zIndex);
                            }
                        }
                    }
                }
            }
        }
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

    public void ZoomAndCenterOn(IEnumerable<MapObject> mapObjects)
    {
        var extent = mapObjects.Select(mapObject => mapObject.ToFeature().Extent).Aggregate((extent1, extent2) => extent1?.Join(extent2));

        _map.Navigator.ZoomToBox(extent?.Grow(1000));
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
}
