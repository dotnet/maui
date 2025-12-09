using System.Globalization;

namespace Maui.Controls.Sample.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }
}
