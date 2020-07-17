using System;

namespace Xamarin.Forms.Platform.UWP
{
	public class BrushConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var brush = value as Brush;
			var color = (Color)parameter;

			return Brush.IsNullOrEmpty(brush) ? color.ToBrush() : brush.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}