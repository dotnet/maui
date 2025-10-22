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
            return parameterValue?.ToLower() switch
            {
                "title" => categoryData.Title,
                "count" => categoryData.Count.ToString(),
                _ => value?.ToString()
            };
        }
        
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
