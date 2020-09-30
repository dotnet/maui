using System;

namespace Xamarin.Forms.Controls
{
	public class GenericValueConverter : IValueConverter
	{
		Func<object, object> _convert;
		Func<object, object> _back;
		public GenericValueConverter(Func<object, object> convert, Func<object, object> back = null)
		{
			_convert = convert;
			_back = back;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return _convert(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return _back(value);
		}
	}
}