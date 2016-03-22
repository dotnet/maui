using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	//TabbedPage -> MasterDetailPage -> NavigationPage -> ContentPage
	public class RootTabbedMDPNavigationContentPage : TabbedPage 
	{
		public RootTabbedMDPNavigationContentPage (string hierarchy) 
		{
			AutomationId = hierarchy + "PageId";

			var tabOne = new MasterDetailPage {
				Title = "Testing 123",
				Master = new ContentPage {
					Title = "Testing 123",
					Content = new StackLayout {
						Children = {
								new Label {Text = "Master"},
								new AbsoluteLayout {
									BackgroundColor = Color.Red,
									VerticalOptions = LayoutOptions.FillAndExpand,
									HorizontalOptions = LayoutOptions.FillAndExpand
								},
								new Button {Text = "Button"}
							}
					}
				},
				Detail = new NavigationPage (new ContentPage {
					Title = "Testing 123",
					Content = new SwapHierachyStackLayout (hierarchy)
				})
			};

			var tabTwo = new ContentPage {
				Title = "Testing 345",
				Content = new StackLayout {
					Children = {
						new Label { Text = "Hello" },
						new AbsoluteLayout {
							BackgroundColor = Color.Red,
							VerticalOptions = LayoutOptions.FillAndExpand,
							HorizontalOptions = LayoutOptions.FillAndExpand
						}
					}
				}
			};

			Children.Add (tabOne);
			Children.Add (tabTwo);
		}
	}
}