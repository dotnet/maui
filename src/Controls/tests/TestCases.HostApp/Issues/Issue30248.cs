namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30248, "TitleBar, MacCatalyst - content is not aligned to left on fullscreen", PlatformAffected.macOS)]

public class Issue30248 : ContentPage
{
	public Issue30248()
	{
		Title = "Issue 30248";

		// Create TitleBar
		var titleBar = new TitleBar
		{
			Title = "Maui App",
			Subtitle = "Hello, World!",
			ForegroundColor = Colors.Red,
			HeightRequest = 48
		};

		titleBar.LeadingContent = new Image {Source = "dotnet_bot.png", HeightRequest = 24};

		// Set the TitleBar on the current Window when this page appears
		this.Loaded += (sender, e) =>
		{
			if (Window != null)
			{
				Window.TitleBar = titleBar;
			}
		};

		// Create the page content with a Label
		Content = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30),
			VerticalOptions = LayoutOptions.Center,
			Children =
				{
					new Label
					{
						Text = "TitleBar should be aligned to the left in fullscreen mode",
						AutomationId = "TitleBarAlignmentLabel",
						FontSize = 32,
						HorizontalOptions = LayoutOptions.Center
					},
				}
		};
	}
}

