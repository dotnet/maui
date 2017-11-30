using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class ColorConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var color = (Color)value;
			var defaultColorKey = (string)parameter;

			Brush defaultBrush = defaultColorKey != null ? (Brush)Windows.UI.Xaml.Application.Current.Resources[defaultColorKey] : new SolidColorBrush(Colors.Transparent);

			return color == Color.Default ? defaultBrush : color.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}