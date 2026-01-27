using Mapsui;
using Mapsui.Layers;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Layers;

/// <summary>
/// Handles management of ordinary writable layers for feature counts <= 50.
/// </summary>
internal class OrdinaryLayerHandler(Map map, Func<IEnumerable<IFeature>, IEnumerable<IFeature>> sortFeatures) : ILayerHandler
{
    public ILayer CreateOrUpdateLayer(IEnumerable<IFeature> features, int zIndex)
    {
        var featuresList = features.ToList();
        
        WritableLayer newLayer = new()
        {
            Name = $"user_{zIndex}",
            SortFeatures = sortFeatures,
            Style = null
        };

        newLayer.AddRange(featuresList);
        return newLayer;
    }

    public IEnumerable<IFeature>? TryRemoveAndGetRemaining(ILayer layer, MapObject mapObject)
    {
        if (layer is not WritableLayer wLayer)
        {
            return null;
        }

        var featuresToRemove = wLayer.GetFeatures()
            .Where(f => Equals(f["mapObject"], mapObject))
            .ToList();

        if (featuresToRemove.Count == 0)
        {
            return null;
        }

        foreach (var feature in featuresToRemove)
        {
            wLayer.TryRemove(feature);
        }

        // Return empty enumerable if layer is now empty, null if nothing was removed
        return wLayer.GetFeatures();
    }

    public IEnumerable<IFeature> GetFeatures(ILayer layer)
    {
        if (layer is WritableLayer wLayer)
        {
            return wLayer.GetFeatures();
        }

        return [];
    }

    public void ApplyFilter(ILayer layer, Func<MapObject, double, bool> filter)
    {
        if (layer is not WritableLayer wLayer)
        {
            return;
        }

        // Create a combined function that filters and sorts
        IEnumerable<IFeature> FilteredAndSorted(IEnumerable<IFeature> features)
        {
            var filtered = features.Where(f =>
            {
                var mapObject = (MapObject?)f["mapObject"];
                return mapObject is not null && filter(mapObject, map.Navigator.Viewport.Resolution);
            });

            return sortFeatures(filtered);
        }

        // Apply the combined filter and sort to the layer
        wLayer.SortFeatures = FilteredAndSorted;
    }

    public bool CanHandle(ILayer layer) => layer is WritableLayer;
}
