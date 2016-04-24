using System;
using System.Globalization;
using Xamarin.Forms;

namespace PagesGallery
{
	public class CapitalizationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var str = (value as string ?? value.ToString());
			return char.ToUpper(str[0]) + str.Substring(1);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}