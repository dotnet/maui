#nullable disable
namespace Maui.Controls.UITests
{
	public class GenericValueConverter : IValueConverter
	{
		readonly Func<object, object> _convert;
		readonly Func<object, object> _back;

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