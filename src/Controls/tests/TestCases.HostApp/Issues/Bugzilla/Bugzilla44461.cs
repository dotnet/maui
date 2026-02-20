namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 44461, "ScrollToPosition.Center works differently on Android and iOS", PlatformAffected.iOS)]
	public class Bugzilla44461 : ContentPage
	{
		const string BtnPrefix = "Button";

		public Bugzilla44461()
		{
			var stackLayout = new StackLayout();


			var instructions = new Label
			{
				Text = @"Tap the first button (Button0). The button should be aligned with the left side of the screen "
				+ "and should not move. If it's not, the test failed."
			};

			stackLayout.Add(instructions);

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Yellow,
				HeightRequest = 50
			};


			var scrollButton = new Button
			{
				Text = "Scroll to Button0",
				AutomationId = "Scroll to Button0",
				Margin = 10,
			};

			stackLayout.Add(scrollButton);
			stackLayout.Add(scrollView);

			var horizontalStack = new StackLayout
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

				horizontalStack.Children.Add(button);
			}

			scrollButton.Clicked += (sender, args) =>
			{
				var button = horizontalStack.Children[0];
				scrollView.ScrollToAsync(button as Element, ScrollToPosition.Center, true);
			};
			scrollView.Content = horizontalStack;
			Content = stackLayout;
		}
	}
}