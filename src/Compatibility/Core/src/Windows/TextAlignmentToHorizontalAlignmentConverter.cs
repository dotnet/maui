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
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed class TextAlignmentToHorizontalAlignmentConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var alignment = (Microsoft.UI.Xaml.TextAlignment)value;

			switch (alignment)
			{
				case Microsoft.UI.Xaml.TextAlignment.Center:
					return HorizontalAlignment.Center;
				case Microsoft.UI.Xaml.TextAlignment.Left:
					return HorizontalAlignment.Left;
				case Microsoft.UI.Xaml.TextAlignment.Right:
					return HorizontalAlignment.Right;
				default:
					return HorizontalAlignment.Left;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			var alignment = (HorizontalAlignment)value;

			switch (alignment)
			{
				case HorizontalAlignment.Left:
					return Microsoft.UI.Xaml.TextAlignment.Left;
				case HorizontalAlignment.Center:
					return Microsoft.UI.Xaml.TextAlignment.Center;
				case HorizontalAlignment.Right:
					return Microsoft.UI.Xaml.TextAlignment.Right;
				default:
					return Microsoft.UI.Xaml.TextAlignment.Left;
			}
		}
	}
}