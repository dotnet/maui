using System.Globalization;

namespace Maui.Controls.Sample.Converters;

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string str && !string.IsNullOrWhiteSpace(str);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
