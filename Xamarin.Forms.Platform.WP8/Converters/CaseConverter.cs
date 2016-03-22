using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class CaseConverter : System.Windows.Data.IValueConverter
	{
		public bool ConvertToUpper { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			var v = (string)value;
			return ConvertToUpper ? v.ToUpper() : v.ToLower();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}