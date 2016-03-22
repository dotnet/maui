using System;
using System.Globalization;
using System.Windows;

namespace Xamarin.Forms.Platform.WinPhone
{
	public sealed class TextAlignmentToHorizontalAlignmentConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var alignment = (System.Windows.TextAlignment)value;

			switch (alignment)
			{
				case System.Windows.TextAlignment.Center:
					return HorizontalAlignment.Center;
				case System.Windows.TextAlignment.Left:
					return HorizontalAlignment.Left;
				case System.Windows.TextAlignment.Right:
					return HorizontalAlignment.Right;
				default:
					return HorizontalAlignment.Left;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var alignment = (HorizontalAlignment)value;

			switch (alignment)
			{
				case HorizontalAlignment.Left:
					return System.Windows.TextAlignment.Left;
				case HorizontalAlignment.Center:
					return System.Windows.TextAlignment.Center;
				case HorizontalAlignment.Right:
					return System.Windows.TextAlignment.Right;
				default:
					return System.Windows.TextAlignment.Left;
			}
		}
	}
}