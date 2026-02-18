using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using SimpleMaps.MapObjects;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

public static class LabelExtensions
{
    public static IFeature ToFeature(this Label label)
    {
        PointFeature feature = new(label.Location.ToMPoint())
        {
            ["mapObject"] = label
        };

        feature.Styles.Add(new LabelStyle()
        {
            Text = label.Text,
            Font = new()
            { 
                Bold = label.Bold, 
                Size = label.FontSize,
                FontFamily = label.FontFamily,
                Italic = label.Italic
            },
            ForeColor = label.FontColor.ToMapsuiColor(),
            BackColor = new(label.BackColor.ToMapsuiColor())
        });

        return feature;
    }
}
