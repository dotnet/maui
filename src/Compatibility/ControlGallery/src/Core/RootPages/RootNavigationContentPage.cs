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
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	// NavigationPage -> ContentPage
	public class RootNavigationContentPage : NavigationPage
	{

		public RootNavigationContentPage(string hierarchy)
		{
			AutomationId = hierarchy + "PageId";

			var content = new ContentPage
			{
				BackgroundColor = Colors.Yellow,
				Title = "Testing 123",
				Content = new SwapHierachyStackLayout(hierarchy)
			};

			PushAsync(content);
		}
	}
}