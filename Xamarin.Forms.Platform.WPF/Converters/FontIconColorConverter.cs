using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.WPF.Converters
{
	public sealed class FontIconColorConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color c && !c.IsDefault)
			{
				return c.ToBrush();
			}

			return Color.White.ToBrush();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}