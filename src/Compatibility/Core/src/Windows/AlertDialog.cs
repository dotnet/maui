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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class AlertDialog : ContentDialog
	{
		public Microsoft.UI.Xaml.Controls.ScrollBarVisibility VerticalScrollBarVisibility { get; set; }

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// The child template name is derived from the default style
			// https://msdn.microsoft.com/en-us/library/windows/apps/mt299120.aspx
			var scrollName = "ContentScrollViewer";
			if (GetTemplateChild(scrollName) is ScrollViewer contentScrollViewer)
				contentScrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
		}
	}
}