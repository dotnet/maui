using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public class StringToBrushConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (string.IsNullOrEmpty(value?.ToString()))
				return null;

			var color = GetColorFromString(value!.ToString()!);
			return new SolidColorBrush(color);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return null;
		}
		Color GetColorFromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return Colors.Transparent;

			try
			{
				return Color.FromArgb(value);
			}
			catch (Exception)
			{
				return Colors.Transparent;
			}
		}
	}
}