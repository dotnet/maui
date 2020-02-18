using System;
using System.Globalization;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF.Converters
{
	public class FontFamilyConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string val && !string.IsNullOrEmpty(val))
			{
				return new FontFamily(new Uri("pack://application:,,,"), val);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}