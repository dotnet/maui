using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
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
							BackgroundColor = Color.Red,
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
