using System;
using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Platform
{
	public class IconConverter : IValueConverter
	{
		object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			PathGeometryConverter converter = new PathGeometryConverter();
			return converter.ConvertFromInvariantString((value as string)!);
		}

		object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			PathGeometryConverter converter = new PathGeometryConverter();
			return converter.ConvertToString(value);
		}
	}
}