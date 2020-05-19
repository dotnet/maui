using System;

namespace System.Maui.Platform.UWP
{
	public class PageToRenderedElementConverter : global::Windows.UI.Xaml.Data.IValueConverter
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