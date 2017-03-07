using System.Diagnostics;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class NavigationMenuGallery : ContentPage
	{
		public NavigationMenuGallery ()
		{
			Content = new ScrollView {
				Content = new NavigationMenu {
					Targets = new[] {
						new ContentPage {
							Content =
								new Label {
									Text = "Test 1",
									VerticalOptions = LayoutOptions.Center,
									HorizontalOptions = LayoutOptions.Center
								},
							Title = "Testing 1",
							Icon = "cover1.jpg"
						},
						new ContentPage { Content = new Label { Text = "Test 2" }, Title = "Testing 2", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 3" }, Title = "Testing 3", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 4" }, Title = "Testing 4", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 5" }, Title = "Testing 5", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 6" }, Title = "Testing 6", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 7" }, Title = "Testing 7", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 8" }, Title = "Testing 8", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 9" }, Title = "Testing 9", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 10" }, Title = "Testing 10", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 11" }, Title = "Testing 11", Icon = "cover1.jpg" },
						new ContentPage { Content = new Label { Text = "Test 12" }, Title = "Testing 12", Icon = "cover1.jpg" }
					}
				}
			};
		}
	}
}