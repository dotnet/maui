using System;
using System.Globalization;


namespace PoolMathApp.Xaml
{
	public class ObjectToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var reverse = false;
			if (parameter?.ToString()?.Equals("invert", StringComparison.OrdinalIgnoreCase) ?? false)
				reverse = true;
			return reverse ? value == null : value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}