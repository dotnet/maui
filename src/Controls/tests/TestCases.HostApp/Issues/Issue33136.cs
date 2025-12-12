using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33136, "TitleBar Content Overlapping with Traffic Light Buttons on Latest macOS Version", PlatformAffected.macOS)]

public class Issue33136 : ContentPage
{
	public Issue33136()
    {
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
						Text = "TitleBar should be aligned properly",
						AutomationId = "TitleBarAlignmentLabel",
						FontSize = 32,
						HorizontalOptions = LayoutOptions.Center
					},
				}
		};
    }
}