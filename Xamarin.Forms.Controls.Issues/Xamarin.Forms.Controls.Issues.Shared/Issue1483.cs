using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1483, "Xamarin.Forms.ActivityIndicator in Forms v 2.5 doesn\'t work (UWP)", PlatformAffected.UWP)]
	public class Issue1483 : TestNavigationPage 
	{
		protected override void Init()
		{
			PushAsync(RootPage());
		}

		ContentPage RootPage()
		{
			var activityIndicator = new ActivityIndicator {VerticalOptions = LayoutOptions.Center, 
				IsRunning = true, IsVisible = true, WidthRequest = 200, HeightRequest = 100 };

			var instructions = new Label { Text = "A running ActivityIndicator should be visible below." 
												+ " If the ActivityIndicator is not visible, this test has failed." };

			var page = new ContentPage();

			var layout = new StackLayout { VerticalOptions = LayoutOptions.Center };

			var button = new Button { Text = "Toggle" };
			button.Clicked += (sender, args) =>
			{
				activityIndicator.IsVisible = !activityIndicator.IsVisible;
				activityIndicator.IsRunning = !activityIndicator.IsRunning;
			};

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(activityIndicator);

			page.Content = layout;

			return page;
		}
	}
}