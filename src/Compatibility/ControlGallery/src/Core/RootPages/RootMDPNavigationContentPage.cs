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
	//FlyoutPage -> NavigationPage -> ContentPage
	public class RootMDPNavigationContentPage : FlyoutPage
	{

		public RootMDPNavigationContentPage(string hierarchy)
		{
			AutomationId = hierarchy + "PageId";

			Flyout = new ContentPage
			{
				Title = "Testing 123",
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Flyout" },
						new AbsoluteLayout {
							BackgroundColor = Colors.Red,
							VerticalOptions = LayoutOptions.FillAndExpand,
							HorizontalOptions = LayoutOptions.FillAndExpand
						},
						new Button { Text = "Button" }
					}
				}
			};

			Detail = new NavigationPage(new ContentPage
			{
				Title = "Md->Nav->Con",
				Content = new SwapHierachyStackLayout(hierarchy)
			});

		}
	}
}
