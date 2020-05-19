using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	
	public class RootTabbedManyNavigationContentPage : TabbedPage 
	{
		public RootTabbedManyNavigationContentPage (string hierarchy) 
		{
			AutomationId = hierarchy + "PageId";

			var page1 = new NavigationPage (new ContentPage {
				Content = new SwapHierachyStackLayout (hierarchy)
			}) {Title = "P 1"};

			var page2 = new NavigationPage (new ContentPage {
				Title = "Page 2",
				Content = new Label {
					Text = "Page 2"
				}
			}) {Title = "P 2"};

			var page3 = new NavigationPage (new ContentPage {
				Title = "Page 3",
				Content = new Label {
					Text = "Page 3"
				}
				
			}) {Title = "P 3"};

			var page4 = new NavigationPage (new ContentPage {
				Title = "Page 4",
				Content = new Label {
					Text = "Page 4"
				}
				
			}) {Title = "P 4"};
			
			var page5 = new NavigationPage (new ContentPage {
				Title = "Page 5",
				Content = new Label {
					Text = "Page 5"
				}
				
			}) {Title = "P 5"};
			
			var page6 = new NavigationPage (new ContentPage {
				Title = "Page 6",
				Content = new Label {
					Text = "Page 6"
				}
				
			}) {Title = "P 6"};

			var page7 = new NavigationPage (new ContentPage {
				Title = "Page 7",
				Content = new Label {
					Text = "Page 7"
				}
				
			}) {Title = "Home"};

			Children.Add (page1);
			Children.Add (page2);
			Children.Add (page3);
			Children.Add (page4);
			Children.Add (page5);
			Children.Add (page6);
			Children.Add (page7);
		}
	}
}