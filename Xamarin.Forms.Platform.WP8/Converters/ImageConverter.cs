using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ImageConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var source = (ImageSource)value;
			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				Task<System.Windows.Media.ImageSource> task = handler.LoadImageAsync(source);
				return new AsyncValue<System.Windows.Media.ImageSource>(task, null);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}