using SimpleMaps.Coordinates;

namespace SimpleMaps;

/// <summary>
/// Contains information about the current map viewport.
/// </summary>
public record ViewportEventArgs(
    WGS84Coordinate Center,
    double Resolution,
    WGS84Coordinate TopLeft,
    WGS84Coordinate TopRight,
    WGS84Coordinate BottomLeft,
    WGS84Coordinate BottomRight);
