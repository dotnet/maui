using System;

namespace Xamarin.Forms.Platform.WPF.Converters
{
	public class ToUpperConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null)
			{
				var strValue = value.ToString();

				return strValue.ToUpperInvariant();
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

	}
}
