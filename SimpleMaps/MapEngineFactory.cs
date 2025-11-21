using SimpleMaps.MapEngine;
using SimpleMaps.MapEngine.Implementations.Mapsui;

namespace SimpleMaps;

public class MapEngineFactory : IMapFactory
{
    public static MapEngineFactory Instance { get; } = new();

    public IMapEngine CreateDefault()
    {
        return CreateMapsuiMapEngine();
    }

    public IMapEngine CreateMapsuiMapEngine()
    {
        return new MapsuiMapEngine();
    }
}
