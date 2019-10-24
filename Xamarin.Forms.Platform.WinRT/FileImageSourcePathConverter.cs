using System;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal class FileImageSourcePathConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var source = (FileImageSource)value;
			string uri = "ms-appx:///" + (source != null ? source.File : string.Empty);
			return new BitmapIcon { UriSource = new Uri(uri) };
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}