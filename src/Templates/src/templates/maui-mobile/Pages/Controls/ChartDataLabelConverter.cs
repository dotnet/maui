using System;
using System.Globalization;
using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public class ChartDataLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CategoryChartData categoryData && parameter is string parameterValue)
        {
            if (string.Equals(parameterValue, "title", StringComparison.OrdinalIgnoreCase))
            {
                return categoryData.Title;
            }

            if (string.Equals(parameterValue, "count", StringComparison.OrdinalIgnoreCase))
            {
                return categoryData.Count.ToString();
            }

            return value?.ToString();
        }

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
