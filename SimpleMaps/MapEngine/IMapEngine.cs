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
}
