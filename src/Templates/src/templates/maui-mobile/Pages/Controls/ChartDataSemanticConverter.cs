using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public class ChartDataSemanticConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<CategoryChartData> categoryDataList)
        {
            var items = categoryDataList.ToList();
            if (items.Count == 0)
                return "No task categories";

            var sb = new StringBuilder("Task Categories Chart: ");
            var descriptions = items.Select(c => $"{c.Title} {c.Count}");
            sb.Append(string.Join(", ", descriptions));

            return sb.ToString();
        }

        return "Task Categories Chart";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}