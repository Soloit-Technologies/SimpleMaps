using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Layers;

/// <summary>
/// Handles management of performance layers (rasterizing tile layers) for feature counts > 50.
/// </summary>
internal class PerformanceLayerHandler(
    Func<IEnumerable<IFeature>, IEnumerable<IFeature>> sortFeatures,
    Dictionary<int, FilteredIndexedMemoryProvider> layerProviders) : ILayerHandler
{
    public ILayer CreateOrUpdateLayer(IEnumerable<IFeature> features, int zIndex)
    {
        var featuresList = features.ToList();
        var provider = new FilteredIndexedMemoryProvider(featuresList)
        {
            Sort = sortFeatures
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
            Name = $"user_{zIndex}",
        };

        layerProviders[zIndex] = provider;

        return tileLayer;
    }

    public IEnumerable<IFeature>? TryRemoveAndGetRemaining(ILayer layer, MapObject mapObject)
    {
        if (layer is not RasterizingTileLayer rasterLayer)
        {
            return null;
        }

        if (rasterLayer.SourceLayer is not Layer sourceLayer)
        {
            return null;
        }

        if (sourceLayer.DataSource is not IndexedMemoryProvider provider)
        {
            return null;
        }

        var featuresToRemove = provider.Features
            .Where(f => Equals(f["mapObject"], mapObject))
            .ToList();

        if (featuresToRemove.Count == 0)
        {
            return null;
        }

        var remainingFeatures = provider.Features
            .Where(f => !Equals(f["mapObject"], mapObject))
            .ToList();

        return remainingFeatures;
    }

    public IEnumerable<IFeature> GetFeatures(ILayer layer)
    {
        if (layer is RasterizingTileLayer rasterTileLayer && rasterTileLayer.SourceLayer is Layer sourceLayer)
        {
            var provider = sourceLayer.DataSource as IndexedMemoryProvider;
            return provider?.Features ?? [];
        }

        return [];
    }

    public void ApplyFilter(ILayer layer, Func<MapObject, double, bool> filter)
    {
        if (layer is not RasterizingTileLayer rasterLayer)
        {
            return;
        }

        // Extract zIndex from layer name to get the provider
        var layerName = rasterLayer.Name;
        if (string.IsNullOrEmpty(layerName) || !layerName.StartsWith("user_"))
        {
            return;
        }

        var layerNameWithoutPrefix = layerName["user_".Length..];
        if (!int.TryParse(layerNameWithoutPrefix, out var zIndex))
        {
            return;
        }

        if (layerProviders.TryGetValue(zIndex, out var provider))
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
        }
    }

    public bool CanHandle(ILayer layer) => layer is RasterizingTileLayer;
}
