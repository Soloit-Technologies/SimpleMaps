using SimpleMaps.Coordinates;
using System.Drawing;

namespace SimpleMaps.MapObjects;

public record Label(Coordinate Location) : MapObject
{
    public string Text { get; init; } = string.Empty;

    public Color FontColor { get; init; } = Color.White;

    public Color BackColor { get; init; } = Color.Black;

    public string FontFamily { get; init; } = "Arial";

    public double FontSize { get; init; } = 12.0;

    public bool Bold { get; init; } = false;
    public bool Italic { get; init; } = false;
}
