using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.WPF
{
	public sealed class ImageConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var source = (ImageSource)value;
			IImageSourceHandler handler;
			
			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				Task<System.Windows.Media.ImageSource> image = handler.LoadImageAsync(source);
				image.Wait();
				return image.Result;
			}

			return null;
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
