using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Providers;

namespace SimpleMaps.MapEngine.Implementations.Mapsui;

internal class FilteredIndexedMemoryProvider(IEnumerable<IFeature> features) : IndexedMemoryProvider(features)
{
    public Func<IFeature, double, bool>? Filter { get; set; }

    public Func<IEnumerable<IFeature>, IEnumerable<IFeature>>? Sort { get; set; }

    public async override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await base.GetFeaturesAsync(fetchInfo);

        var filteredFeatures = Filter is null ? features : features.Where(f => Filter(f, fetchInfo.Resolution));

        return Sort is null ? filteredFeatures : Sort(filteredFeatures);
    }
}
