using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Providers;

namespace SimpleMaps.MapEngine.Implementations.Mapsui;

internal class FilteredIndexedMemoryProvider(IEnumerable<IFeature> features) : IndexedMemoryProvider(features)
{
    private Func<IFeature, double, bool>? _filter;

    public void SetFilter(Func<IFeature, double, bool>? filter) => _filter = filter;

    public async override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await base.GetFeaturesAsync(fetchInfo);

        return _filter is null ? features : features.Where(f => _filter(f, fetchInfo.Resolution));
    }
}
