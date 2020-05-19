using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	// MasterDetailPage -> NavigationPage -> TabbedPage -> ContentPage
	public class RootMDPNavigationTabbedContentPage : MasterDetailPage {

		public RootMDPNavigationTabbedContentPage (string hierarchy)
		{
			AutomationId = hierarchy + "PageId";


			var tabbedPage = new TabbedPage ();

			var firstTab = new ContentPage {
				//BackgroundColor = Color.Yellow,
				Title = "Testing 123",
				Content = new SwapHierachyStackLayout (hierarchy)
			};

			tabbedPage.Children.Add (firstTab);

			NavigationPage.SetHasNavigationBar (firstTab, false);

			Detail = new NavigationPage (tabbedPage);
			Master = new NavigationPage (new ContentPage {
				Title = "Testing 345",
				Content = new StackLayout {
					Children = {
						new Label { Text = "Hello" },
						new AbsoluteLayout {
							BackgroundColor = Color.Red,
							VerticalOptions = LayoutOptions.FillAndExpand,
							HorizontalOptions = LayoutOptions.FillAndExpand
						},
						new Button { Text = "Button" }
					}
				}
			}) { 
				Title = "Testing 345"
			};
		}
	}
}
