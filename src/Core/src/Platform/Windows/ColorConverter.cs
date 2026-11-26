using System;
using Microsoft.UI;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public sealed partial class ColorConverter : UI.Xaml.Data.IValueConverter
	{
		internal const string ThemeResourceFallback = "ThemeResourceFallback";

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter is ThemeResourceFallback)
				return ConvertWithThemeResourceFallback((Graphics.Color?)value);

			var color = (Graphics.Color)value;
			var defaultColorKey = (string)parameter;

			WBrush defaultBrush = defaultColorKey != null ?
				(WBrush)UI.Xaml.Application.Current.Resources[defaultColorKey] :
				new WSolidColorBrush(Colors.Transparent);

			return color.IsDefault() ? defaultBrush : color.ToPlatform();
		}

		internal static object ConvertWithThemeResourceFallback(Graphics.Color? color) =>
			color is null ? UI.Xaml.DependencyProperty.UnsetValue : color.ToPlatform();

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}