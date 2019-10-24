using System;
using System.Globalization;
using System.Windows;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class CollapseWhenEmptyConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var length = 0;

			var s = value as string;
			if (s != null)
				length = s.Length;

			if (value is int)
				length = (int)value;

			return length > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}