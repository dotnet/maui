using System;
using System.Globalization;


namespace PoolMathApp.Xaml
{
	public class BoolToGridLengthConverter : IValueConverter
	{
		public BoolToGridLengthConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Boolean && !(bool)value)
				return new GridLength(0);

			if (parameter?.ToString()?.Equals("auto", StringComparison.OrdinalIgnoreCase) ?? false)
				return GridLength.Auto;

			double numParam = -1;
			if (parameter != null && double.TryParse(parameter.ToString(), out numParam))
				return new GridLength(numParam, GridUnitType.Absolute);

			return new GridLength(1, GridUnitType.Star);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
