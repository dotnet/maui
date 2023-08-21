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

namespace Microsoft.Maui.Controls.ControlGallery
{
	// May not behave
	// NavigationPage with multiple tabbed pages	
	public class CaseTenPage : FlyoutPage
	{
		public CaseTenPage()
		{
			var btn = new Button { Text = "Click Me" };
			btn.Clicked += (sender, args) => btn.Navigation.PushModalAsync(new NavigationPage(new ContentPage()));

			var detail = new ContentPage { Content = btn };

			NavigationPage.SetHasNavigationBar(detail, false);

			Flyout = new ListPage() { Title = "Flyout" };
			Detail = detail;
		}
	}
}