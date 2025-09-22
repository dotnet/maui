using System.Globalization;
using Microsoft.Maui.Controls;
using MauiApp._1.Models;

namespace MauiApp._1.Converter;

public class DataLabelValueConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is CategoryChartData categoryChartData)
		{
			switch (parameter?.ToString())
			{
				case "Title":
					return categoryChartData.Title;

				case "Count":
					return categoryChartData.Count;
			}
		}

		return value;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value;
	}
}
