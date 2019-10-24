using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class HorizontalTextAlignmentConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var textAlign = (TextAlignment)value;

			return textAlign.ToNativeTextAlignment();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}