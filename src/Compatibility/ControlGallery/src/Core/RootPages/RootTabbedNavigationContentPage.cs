using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class RootTabbedNavigationContentPage : TabbedPage
	{
		public RootTabbedNavigationContentPage(string hierarchy)
		{
			AutomationId = hierarchy + "PageId";

			var tabOne = new NavigationPage(new ContentPage
			{
				Title = "Nav title",
				Content = new SwapHierachyStackLayout(hierarchy)
			})
			{ Title = "Tab 123", };

			var tabTwo = new ContentPage
			{
				Title = "Testing 345",
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Hello" },
						new AbsoluteLayout {
							BackgroundColor = Color.Red,
							VerticalOptions = LayoutOptions.FillAndExpand,
							HorizontalOptions = LayoutOptions.FillAndExpand
						},
						new Button { Text = "Button" },
					}
				}
			};

			Children.Add(tabOne);
			Children.Add(tabTwo);
		}
	}
}

