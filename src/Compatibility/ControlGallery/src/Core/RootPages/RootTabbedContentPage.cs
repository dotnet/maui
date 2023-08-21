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
	// TabbedPage -> ContentPage
	public class RootTabbedContentPage : TabbedPage
	{
		public RootTabbedContentPage(string hierarchy)
		{
			AutomationId = hierarchy + "PageId";

			var tabOne = new ContentPage
			{
				Title = "Testing 123",
				Content = new SwapHierachyStackLayout(hierarchy)
			};

			var clearSelectedTabColorButton = new Button { Text = "Button" };
			clearSelectedTabColorButton.Clicked += (s, a) =>
			{
				UnselectedTabColor = null;
				SelectedTabColor = null;
			};

			var tabTwo = new ContentPage
			{
				Title = "Testing 345",
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Hello" },
						new AbsoluteLayout {
							BackgroundColor = Colors.Red,
							VerticalOptions = LayoutOptions.FillAndExpand,
							HorizontalOptions = LayoutOptions.FillAndExpand
						}, clearSelectedTabColorButton
					}
				}
			};

			UnselectedTabColor = Colors.HotPink;
			SelectedTabColor = Colors.Green;

			Children.Add(tabOne);
			Children.Add(tabTwo);
		}
	}
}