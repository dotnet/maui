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
	// TabbedPage -> NavigationPage
	public class TabbedNavPage : FlyoutPage
	{
		public TabbedNavPage()
		{
			NavigationPage navpage = null;
			navpage = new NavigationPage(new ContentPage
			{
				Title = "Content0",
				Content = new Button
				{
					Text = "Button",
					Command = new Command(() =>
					{
						Debug.WriteLine("k");
						navpage.PushAsync(new ContentPage() { Title = "Content1" });
					})
				}
			})
			{
				Title = "Page0",
			};

			//Children.add(navpage);
			//Children.add(new HomeButton ());
		}
	}
}