using System;
using global::Windows.UI;
using global::Windows.UI.Xaml.Media;

namespace System.Maui.Platform.UWP
{
	public sealed class ColorConverter : global::Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var color = (Color)value;
			var defaultColorKey = (string)parameter;

			Brush defaultBrush = defaultColorKey != null ? (Brush)global::Windows.UI.Xaml.Application.Current.Resources[defaultColorKey] : new SolidColorBrush(Colors.Transparent);

			return color == Color.Default ? defaultBrush : color.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}