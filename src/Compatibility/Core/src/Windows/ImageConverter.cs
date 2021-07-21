using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ImageConverter : Microsoft.UI.Xaml.Data.IValueConverter
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