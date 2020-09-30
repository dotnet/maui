using System;
using Windows.UI;
using WBrush = Windows.UI.Xaml.Media.Brush;
using WSolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class ColorConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var color = (Color)value;
			var defaultColorKey = (string)parameter;

			WBrush defaultBrush = defaultColorKey != null ? (WBrush)Windows.UI.Xaml.Application.Current.Resources[defaultColorKey] : new WSolidColorBrush(Colors.Transparent);

			return color == Color.Default ? defaultBrush : color.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}