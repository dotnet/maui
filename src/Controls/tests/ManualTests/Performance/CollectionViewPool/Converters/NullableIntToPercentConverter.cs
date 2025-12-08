using System;
using System.Globalization;


namespace PoolMathApp.Xaml
{
	public class NullableIntToPercentConverter : IValueConverter
	{
		public NullableIntToPercentConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int?))
				return string.Empty;

			var nt = (int?)value;

			if (!nt.HasValue)
				return string.Empty;

			return nt.Value.ToString("0") + "%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
