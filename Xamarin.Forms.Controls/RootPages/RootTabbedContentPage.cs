using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
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
				UnselectedTabColor = Color.Default;
				SelectedTabColor = Color.Default;
			};

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
						}, clearSelectedTabColorButton
					}
				}
			};

			UnselectedTabColor = Color.HotPink;
			SelectedTabColor = Color.Green;

			Children.Add(tabOne);
			Children.Add(tabTwo);
		}
	}
}