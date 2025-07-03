using System;
using System.Globalization;


namespace PoolMathApp.Xaml
{
	public class NullableDoubleToTemperatureConverter : IValueConverter
	{
		public NullableDoubleToTemperatureConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double?))
				return string.Empty;

			var dbl = (double?)value;

			if (!dbl.HasValue)
				return string.Empty;

			return dbl.Value.ToString("0") + "º";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
