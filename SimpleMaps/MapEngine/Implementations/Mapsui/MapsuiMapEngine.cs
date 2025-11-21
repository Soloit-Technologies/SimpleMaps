using Mapsui;
using Mapsui.Animations;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Tiling.Layers;
using SimpleMaps.Coordinates;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine.Implementations.Mapsui;

internal class MapsuiMapEngine : IMapEngine
{
    private readonly Map _map = new();

    private readonly MyLocationLayer _positionLayer;

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
        _positionLayer = new(_map)
        {
            Enabled = false,
            IsCentered = false
        };
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
        Layer layer = new()
        {
            DataSource = new FilteredIndexedMemoryProvider(features),
            SortFeatures = SortFeatures
        };

        RasterizingTileLayer tileLayer = new(layer)
        {
            Name = zIndex.ToString()
        };

        ReplaceLayer(tileLayer, zIndex);
    }

    private void ReplaceLayer(RasterizingTileLayer layer, int zIndex)
    {
        var layerToBeRemoved =_map.Layers.FirstOrDefault(l => l.Name == zIndex.ToString());
        layer.Enabled = layerToBeRemoved?.Enabled ?? true;

        if (layerToBeRemoved is not null)
        {
            _map.Layers.Remove(layerToBeRemoved);
        }
        
        // Find the correct position to insert based on logical layer indices
        var insertPosition = 0;
        foreach (var existingLayer in _map.Layers)
        {
            if (int.TryParse(existingLayer.Name, out var existingIndex) && existingIndex < zIndex)
            {
                insertPosition++;
            }
        }
        
        _map.Layers.Insert(insertPosition, layer);
    }

    private IReadOnlyList<IFeature> GetFeatures(int layerIndex)
    {
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerIndex.ToString());
        if (layer == null)
        {
            return [];
        }

        var features = ((IndexedMemoryProvider?)((Layer)layer).DataSource)!.Features;
        
        return features;
    }

    private static IEnumerable<IFeature> SortFeatures(IEnumerable<IFeature> features)
    {
        return features.OrderBy(f => ((MapObject?)f["mapObject"])?.RenderingOrder);
    }

    public void SetFilter(int layerIndex, Func<MapObject, double, bool> filter)
    {
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerIndex.ToString());
        if (layer is not null)
        {
            var provider = (FilteredIndexedMemoryProvider?)((Layer)layer).DataSource;
            provider?.SetFilter((feature, resolution) =>
            {
                var mapObject = (MapObject?)feature["mapObject"];
                if (mapObject is null)
                {
                    return false;
                }

                return filter(mapObject, resolution);
            });
        }
    }

    public void Refresh()
    {
        foreach (var layer in _map.Layers)
        {
            ((RasterizingTileLayer)layer).ClearCache();
        }
    }

    public void SetVisible(int layerIndex, bool enable = true)
    {
        var layer = _map.Layers.FirstOrDefault(l => l.Name == layerIndex.ToString());
        layer?.Enabled = enable;
    }

    public void RemoveAll(int zIndex = 0)
    {
        var layer = _map.Layers.FirstOrDefault(l => l.Name == zIndex.ToString());
        if (layer is not null)
        {
            _map.Layers.Remove(layer);
        }
    }

    public void Remove(MapObject mapObject)
    {
        foreach (var layer in _map.Layers.ToList())
        {
            if (layer.Name is not null && int.TryParse(layer.Name, out var zIndex))
            {
                if (((Layer)layer).DataSource is IndexedMemoryProvider provider)
                {
                    var remainingFeatures = provider.Features
                        .Where(f => !Equals(f["mapObject"], mapObject))
                        .ToList();

                    if (remainingFeatures.Count != provider.Features.Count)
                    {
                        if (remainingFeatures.Count == 0)
                        {
                            _map.Layers.Remove(layer);
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
}
