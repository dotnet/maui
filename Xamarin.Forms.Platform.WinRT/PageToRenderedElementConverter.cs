using System;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class PageToRenderedElementConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var page = value as Page;
			if (page == null)
				return null;

			IVisualElementRenderer renderer = page.GetOrCreateRenderer();
			if (renderer == null)
				return null;

			return renderer.ContainerElement;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}