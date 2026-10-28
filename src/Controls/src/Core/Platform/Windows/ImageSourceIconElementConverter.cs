#nullable disable
using System;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ImageSourceIconElementConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is ImageSource source)
				return source.ToIconSource(source.FindMauiContext())?.CreateIconElement();

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}