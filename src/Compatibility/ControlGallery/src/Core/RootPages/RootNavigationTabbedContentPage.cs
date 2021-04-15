using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
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