using System;
using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class TextAlignmentToHorizontalAlignmentConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var alignment = (Windows.UI.Xaml.TextAlignment)value;

			switch (alignment)
			{
				case Windows.UI.Xaml.TextAlignment.Center:
					return HorizontalAlignment.Center;
				case Windows.UI.Xaml.TextAlignment.Left:
					return HorizontalAlignment.Left;
				case Windows.UI.Xaml.TextAlignment.Right:
					return HorizontalAlignment.Right;
				default:
					return HorizontalAlignment.Left;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			var alignment = (HorizontalAlignment)value;

			switch (alignment)
			{
				case HorizontalAlignment.Left:
					return Windows.UI.Xaml.TextAlignment.Left;
				case HorizontalAlignment.Center:
					return Windows.UI.Xaml.TextAlignment.Center;
				case HorizontalAlignment.Right:
					return Windows.UI.Xaml.TextAlignment.Right;
				default:
					return Windows.UI.Xaml.TextAlignment.Left;
			}
		}
	}
}