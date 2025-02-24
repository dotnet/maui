
namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22075, "Button rendering overflow issue when padding is set in StackLayout", PlatformAffected.UWP)]
	public class Issue22075 : ContentPage
	{
		public Issue22075()
		{
			StackLayout stackLayout = new StackLayout
			{
				Padding = 15,
				BackgroundColor = Colors.Green,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			Button button = new Button
			{
				Text = "Click me",
				AutomationId = "Button",
				BackgroundColor = Colors.Violet
			};

			stackLayout.Children.Add(button);

			Content = stackLayout;
		}
	}
}
