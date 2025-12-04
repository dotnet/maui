using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform
{
	public sealed partial class TextAlignmentToHorizontalAlignmentConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var alignment = (Microsoft.UI.Xaml.TextAlignment)value;

			switch (alignment)
			{
				case Microsoft.UI.Xaml.TextAlignment.Center:
					return HorizontalAlignment.Center;
				case Microsoft.UI.Xaml.TextAlignment.Justify:
					return HorizontalAlignment.Stretch;
				case Microsoft.UI.Xaml.TextAlignment.Left:
					return HorizontalAlignment.Left;
				case Microsoft.UI.Xaml.TextAlignment.Right:
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
					return Microsoft.UI.Xaml.TextAlignment.Left;
				case HorizontalAlignment.Center:
					return Microsoft.UI.Xaml.TextAlignment.Center;
				case HorizontalAlignment.Stretch:
					return Microsoft.UI.Xaml.TextAlignment.Justify;
				case HorizontalAlignment.Right:
					return Microsoft.UI.Xaml.TextAlignment.Right;
				default:
					return Microsoft.UI.Xaml.TextAlignment.Left;
			}
		}
	}
}