using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class HorizontalTextAlignmentConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var textAlign = (TextAlignment)value;
			return textAlign.ToNativeTextAlignment();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}