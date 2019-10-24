using System;
using System.Threading.Tasks;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ImageConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var source = (ImageSource)value;
			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				Task<Windows.UI.Xaml.Media.ImageSource> task = handler.LoadImageAsync(source);
				return new AsyncValue<Windows.UI.Xaml.Media.ImageSource>(task, null);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}