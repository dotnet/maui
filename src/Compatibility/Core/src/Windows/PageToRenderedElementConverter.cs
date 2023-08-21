//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class PageToRenderedElementConverter : Microsoft.UI.Xaml.Data.IValueConverter
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