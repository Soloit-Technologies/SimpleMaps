using Mapsui;
using Mapsui.Layers;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Layers;

/// <summary>
/// Defines the contract for handling different types of map layers.
/// </summary>
internal interface ILayerHandler
{
    /// <summary>
    /// Creates or updates a layer with the specified features.
    /// </summary>
    /// <param name="features">The features to add to the layer.</param>
    /// <param name="zIndex">The z-index of the layer.</param>
    /// <returns>The created or updated layer.</returns>
    ILayer CreateOrUpdateLayer(IEnumerable<IFeature> features, int zIndex);

    /// <summary>
    /// Removes a map object from the layer and returns the remaining features (if any).
    /// </summary>
    /// <param name="layer">The layer to remove from.</param>
    /// <param name="mapObject">The map object to remove.</param>
    /// <returns>The remaining features, or null if nothing was found.</returns>
    IEnumerable<IFeature>? TryRemoveAndGetRemaining(ILayer layer, MapObject mapObject);

    /// <summary>
    /// Gets all features from the layer.
    /// </summary>
    /// <param name="layer">The layer to get features from.</param>
    /// <returns>An enumerable of features.</returns>
    IEnumerable<IFeature> GetFeatures(ILayer layer);

    /// <summary>
    /// Applies a filter to the layer.
    /// </summary>
    /// <param name="layer">The layer to apply the filter to.</param>
    /// <param name="filter">The filter function.</param>
    void ApplyFilter(ILayer layer, Func<MapObject, double, bool> filter);

    /// <summary>
    /// Determines if this handler can handle the given layer type.
    /// </summary>
    /// <param name="layer">The layer to check.</param>
    /// <returns>True if this handler can manage the layer; otherwise, false.</returns>
    bool CanHandle(ILayer layer);
}
