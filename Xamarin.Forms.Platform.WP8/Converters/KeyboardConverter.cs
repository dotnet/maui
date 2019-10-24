using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class KeyboardConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var keyboard = (Keyboard)value;
			return keyboard.ToInputScope();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}