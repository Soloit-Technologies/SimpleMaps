using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects;
using SimpleMaps.Tiles;

namespace SimpleMaps.MapEngine;

public interface IMapEngine
{
    /// <summary>
    /// Gets the underlying native map object (e.g. Mapsui.Map) for platform-specific integration.
    /// </summary>
    object NativeMap { get; }

    /// <summary>
    /// Raised when the viewport center or resolution changes.
    /// </summary>
    event EventHandler<ViewportEventArgs>? ViewportChanged;

    /// <summary>
    /// Raised when the user clicks/taps on the map.
    /// </summary>
    event EventHandler<MapEventArgs>? MapClicked;

    public bool ShowLocationMarker { get; set; }

    public Coordinate MyLocation { get; }

    public bool IsCentered { get; set; }

    public void SetFilter(int zIndex, Func<MapObject, double, bool> filter);

    public void Refresh();

    public void SetVisible(int zIndex, bool enable = true);

    void Add(MapObject mapObject, int zIndex = 0);

    void Add(IEnumerable<MapObject> mapObjects, int zIndex = 0);

    void Replace(IEnumerable<MapObject> mapObjects, int zIndex = 0);

    public void Remove(MapObject mapObject);

    public void RemoveAll(int zIndex = 0);

    public void MoveLocationMarker(Coordinate location, double bearing);

    public void ZoomIn();

    public void ZoomOut();

    public void CenterOn(Coordinate location);

    /// <summary>
    /// Centers the map on the given location at the specified resolution (zoom level).
    /// </summary>
    public void CenterOn(Coordinate location, double resolution);

    /// <summary>
    /// Animates a fly-to to the given location.
    /// </summary>
    public void FlyTo(Coordinate location);

    public void ZoomAndCenterOn(IEnumerable<MapObject> mapObjects);

    void AddTileLayer(ITileProvider provider, int zIndex);

    void RemoveTileLayer(int zIndex);

    void SetTileLayerVisible(int zIndex, bool visible = true);

    void ClearTileLayerCache(int zIndex);

    /// <summary>
    /// Adds a WMS layer to the map at the specified z-index.
    /// </summary>
    /// <param name="url">The WMS GetCapabilities URL.</param>
    /// <param name="layerName">The WMS layer name to display.</param>
    /// <param name="zIndex">The z-index for layer ordering.</param>
    /// <param name="proxyBaseUrl">Optional proxy base URL. When set, all WMS requests are routed through this proxy
    /// by replacing the original WMS host with this URL. The proxy is responsible for authentication.</param>
    Task AddWmsLayerAsync(string url, string layerName, int zIndex, string? proxyBaseUrl = null);

    /// <summary>
    /// Removes a WMS layer at the specified z-index.
    /// </summary>
    void RemoveWmsLayer(int zIndex);

    /// <summary>
    /// Sets the visibility of a WMS layer.
    /// </summary>
    void SetWmsLayerVisible(int zIndex, bool visible = true);
}
