using System.Drawing;

namespace SimpleMaps.MapObjects.Styles;

public record Pen(Color Color)
{
    public double Width { get; init; } = 1.0;
    public Stroke Stroke { get; init; } = Stroke.Solid;
}
