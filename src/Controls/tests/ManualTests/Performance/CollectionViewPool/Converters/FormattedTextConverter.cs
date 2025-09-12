using System;
using System.Globalization;
using PoolMathApp.Helpers;
using PoolMathApp.Models;


namespace PoolMathApp.Xaml
{
	public class FormattedTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (value as FormattedText)?.ToFormattedString();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> -1;
	}
}
