using System;
using Microsoft.UI;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public sealed class ColorConverter : UI.Xaml.Data.IValueConverter
	{
		public static object Convert(object value, Type targetType, object parameter, string language)
		{
			var color = (Graphics.Color)value;
			var defaultColorKey = (string)parameter;

			WBrush defaultBrush = defaultColorKey != null ?
				(WBrush)UI.Xaml.Application.Current.Resources[defaultColorKey] :
				new WSolidColorBrush(Colors.Transparent);

			return color.IsDefault() ? defaultBrush : color.ToPlatform();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}