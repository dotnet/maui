using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class PageToRendererConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var page = value as Page;
			if (page == null)
				return null;

			IVisualElementRenderer renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);
			return renderer;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}