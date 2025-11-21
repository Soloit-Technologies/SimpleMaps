using Mapsui.Styles;
using SimpleMaps.MapObjects.Styles;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class StrokeExtensions
{
    public static PenStyle ToPenStyle(this Stroke stroke) => stroke switch
    {
        Stroke.Solid => PenStyle.Solid,
        Stroke.Dashed => PenStyle.Dash,
        Stroke.ShortDashed => PenStyle.ShortDash,
        _ => throw new ArgumentException("Stroke not supported.")
    };
}
