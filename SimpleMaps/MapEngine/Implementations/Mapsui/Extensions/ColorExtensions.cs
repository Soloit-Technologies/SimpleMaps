using Mapsui.Styles;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class ColorExtensions
{
    public static Color ToMapsuiColor(this System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);
}
