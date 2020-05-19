using System;

namespace System.Maui.Platform.UWP
{
	public class HorizontalTextAlignmentConverter : global::Windows.UI.Xaml.Data.IValueConverter
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