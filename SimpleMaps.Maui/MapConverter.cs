using SimpleMaps.MapEngine.Implementations.Mapsui;
using System.Globalization;

namespace SimpleMaps.Maui;

internal class MapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((MapsuiMapEngine?)value)?.MapsuiMap;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
