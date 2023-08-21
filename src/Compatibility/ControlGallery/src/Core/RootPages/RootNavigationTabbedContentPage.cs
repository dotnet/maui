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
	// NavigationPage -> TabbedPage -> ContentPage
	// Not recommended
	public class RootNavigationTabbedContentPage : NavigationPage
	{
		public RootNavigationTabbedContentPage(string hierarchy)
		{
			AutomationId = hierarchy + "PageId";

			var tabbedPage = new TabbedPage
			{
				Children = {
					new ContentPage {
						Title = "Page 1",
						Content = new SwapHierachyStackLayout (hierarchy)
					},
					new ContentPage {
						Title = "Page 2",
						Content = new StackLayout {
							Children = {
								new Label { Text = "Page Two" },
								new BoxView {
									Color = Colors.Gray,
									VerticalOptions = LayoutOptions.FillAndExpand,
									HorizontalOptions = LayoutOptions.FillAndExpand
								},
								new Button { Text = "Click me" },
							}
						}
					}
				}
			};

			PushAsync(tabbedPage);
		}

	}
}