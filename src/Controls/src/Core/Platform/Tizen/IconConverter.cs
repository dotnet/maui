using System;
using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Platform
{
	public class IconConverter : IValueConverter
	{
		private static readonly PathGeometryConverter PathGeometryConverter = new();

		object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return PathGeometryConverter.ConvertFromInvariantString((value as string)!);
		}

		object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return PathGeometryConverter.ConvertToString(value);
		}
	}
}