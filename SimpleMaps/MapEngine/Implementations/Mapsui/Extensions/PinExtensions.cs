using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using SimpleMaps.MapObjects;
using Color = Mapsui.Styles.Color;

namespace SimpleMaps.MapEngine.Implementations.Mapsui.Extensions;

internal static class PinExtensions
{
    public static IFeature ToFeature(this Pin pin)
    {
        PointFeature feature = new(pin.Location.ToMPoint())
        {
            ["mapObject"] = pin
        };

        feature.Styles.Add(CreatePinSymbol(pin.Color));

        return feature;
    }

    private static ImageStyle CreatePinSymbol(System.Drawing.Color color) => new()
    {
        Image = new()
        {
            Source = "embedded://Mapsui.Resources.Images.pin.svg",
            SvgFillColor = color.ToMapsuiColor(),
            SvgStrokeColor = Color.DimGrey,
        },
        RelativeOffset = new RelativeOffset(0.0, 0.5), // The symbols point should be at the geolocation.        
    };
}
