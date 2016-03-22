using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
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