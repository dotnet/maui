using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Controls.Sample.Converters
{
	public class BoolToCustomValueConverter : IValueConverter
	{
		public object ValueIfTrue { get; set; }
		public object ValueIfFalse { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool boolValue && boolValue ? ValueIfTrue : ValueIfFalse;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}