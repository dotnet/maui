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
                return "Task Categories Chart with no categories.";

            var categoryLabel = items.Count == 1 ? "category" : "categories";
            var sb = new StringBuilder($"Task Categories Chart with {items.Count} {categoryLabel}: ");
            var descriptions = items.Select(c =>
                $"{c.Title} has {c.Count} {(c.Count == 1 ? "task" : "tasks")}");
            sb.Append(string.Join(", ", descriptions));

            return sb.ToString();
        }

        return "Task Categories Chart showing task categories.";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}