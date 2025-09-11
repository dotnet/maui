using System.Globalization;

namespace Maui.Controls.Sample;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var currentEnum = value.ToString();
        var targetEnum = parameter.ToString();

        return string.Equals(currentEnum, targetEnum, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            if (Enum.TryParse(targetType, parameter.ToString(), out var result))
            {
                return result;
            }
        }
        return Binding.DoNothing;
    }
}