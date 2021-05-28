using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI;
using Windows.UI;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed class ColorConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var color = (Microsoft.Maui.Graphics.Color)value;
			var defaultColorKey = (string)parameter;

			WBrush defaultBrush = defaultColorKey != null ? (WBrush)Microsoft.UI.Xaml.Application.Current.Resources[defaultColorKey] : new WSolidColorBrush(Colors.Transparent);

			return color.IsDefault() ? defaultBrush : Maui.ColorExtensions.ToNative(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}