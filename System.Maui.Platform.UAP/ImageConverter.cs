using System;
using System.Threading.Tasks;
using System.Maui.Internals;

namespace System.Maui.Platform.UWP
{
	public class ImageConverter : global::Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value is ImageSource source
				? source.ToWindowsImageSourceAsync().AsAsyncValue()
				: null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}