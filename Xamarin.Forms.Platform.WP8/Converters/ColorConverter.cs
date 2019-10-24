using System;
using System.Globalization;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ColorConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (Color)value;
			var defaultColorKey = (string)parameter;

			Brush defaultBrush = defaultColorKey != null ? (Brush)System.Windows.Application.Current.Resources[defaultColorKey] : new SolidColorBrush(Colors.Transparent);

			return color == Color.Default ? defaultBrush : color.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}