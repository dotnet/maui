using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	// May not behave
	// NavigationPage with multiple tabbed pages	
	public class RootNavigationManyTabbedPage : NavigationPage 
	{
		public RootNavigationManyTabbedPage (string hierarchy) 
		{
			AutomationId = hierarchy + "PageId";

			var content = new TabbedPage {
				Children = { 
					new ContentPage {
						Title = "Page 1",
						Content = new SwapHierachyStackLayout (hierarchy)
					},
					new ContentPage {
						Title = "Page 2",
						Content = new Label {
							Text = "Page 2"
						}
					},
					new ContentPage {
						Title = "Page 3",
						Content = new Label {
							Text = "Page 3"
						}
					},
					new ContentPage {
						Title = "Page 4",
						Content = new Label {
							Text = "Page 4"
						}
					},
					new ContentPage {
						Title = "Page 5",
						Content = new Label {
							Text = "Page 5"
						}
					},
					new ContentPage {
						Title = "Page 6",
						Content = new Label {
							Text = "Page 6"
						}
					}, 
					
					new ContentPage {
						Title = "Go Home",
					}, 
				}
			};
			PushAsync (content);
		}
	}	
}