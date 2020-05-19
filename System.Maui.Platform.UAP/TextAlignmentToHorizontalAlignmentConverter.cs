using System;
using global::Windows.UI.Xaml;

namespace System.Maui.Platform.UWP
{
	public sealed class TextAlignmentToHorizontalAlignmentConverter : global::Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var alignment = (global::Windows.UI.Xaml.TextAlignment)value;

			switch (alignment)
			{
				case global::Windows.UI.Xaml.TextAlignment.Center:
					return HorizontalAlignment.Center;
				case global::Windows.UI.Xaml.TextAlignment.Left:
					return HorizontalAlignment.Left;
				case global::Windows.UI.Xaml.TextAlignment.Right:
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
					return global::Windows.UI.Xaml.TextAlignment.Left;
				case HorizontalAlignment.Center:
					return global::Windows.UI.Xaml.TextAlignment.Center;
				case HorizontalAlignment.Right:
					return global::Windows.UI.Xaml.TextAlignment.Right;
				default:
					return global::Windows.UI.Xaml.TextAlignment.Left;
			}
		}
	}
}