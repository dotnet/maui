using System;
using System.Globalization;


namespace PoolMathApp.Xaml
{
	public class NullableDoubleToBoolConverter : IValueConverter
	{
		public NullableDoubleToBoolConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter is bool pbool && !pbool)
				return false;

			if (value is double?)
			{
				var ndbl = value as double?;
				return ndbl.HasValue;
			}

			if (value is double)
			{
				return true;
			}

			if (value is int?)
			{
				var nint = value as int?;
				return nint.HasValue;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
