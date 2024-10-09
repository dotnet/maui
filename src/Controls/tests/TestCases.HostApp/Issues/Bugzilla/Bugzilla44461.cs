using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 44461, "ScrollToPosition.Center works differently on Android and iOS", PlatformAffected.iOS)]
	public class Bugzilla44461 : ContentPage
	{
		const string BtnPrefix = "Button";

		public Bugzilla44461()
		{
			var grid = new Grid
			{
				RowSpacing = 0,
			};

			var instructions = new Label
			{
				Text = @"Tap the first button (Button0). The button should be aligned with the left side of the screen "
				+ "and should not move. If it's not, the test failed."
			};

			grid.Children.Add(instructions);

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Yellow,
				HeightRequest = 50
			};
			grid.Children.Add(scrollView);

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 20
			};

			for (var i = 0; i < 10; i++)
			{
				var button = new Button
				{
					AutomationId = $"{i}",
					Text = $"{BtnPrefix}{i}"
				};
				button.Clicked += (sender, args) =>
				{
					scrollView.ScrollToAsync(sender as Button, ScrollToPosition.Center, true);
				};

				stackLayout.Children.Add(button);
			}
			scrollView.Content = stackLayout;
			Content = grid;
		}
	}
}
