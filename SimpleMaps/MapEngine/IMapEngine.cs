using SimpleMaps.Coordinates;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine;

public interface IMapEngine
{
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

    public void ZoomAndCenterOn(IEnumerable<MapObject> mapObjects);
}
