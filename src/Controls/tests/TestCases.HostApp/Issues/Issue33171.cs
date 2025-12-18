namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33171, "When TitleBar.IsVisible = false the caption buttons become unresponsive on Windows", PlatformAffected.UWP)]
public class Issue33171 : ContentPage
{
    public Issue33171()
	{
		Title = "Issue 33171";

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

		// Create a label to display window size
		var windowSizeLabel = new Label
		{
			Text = "Window Size: Not set",
			AutomationId = "WindowSizeLabel",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center
		};

		// Create a button to reduce window width
		var reduceWidthButton = new Button
		{
			Text = "Reduce Window Width",
			AutomationId = "ReduceWidthButton",
			HorizontalOptions = LayoutOptions.Center
		};

		reduceWidthButton.Clicked += (sender, e) =>
		{
			if (Window != null)
			{
				// Reduce window width by 50 pixels
				var currentWidth = Window.Width;
				var newWidth = Math.Max(currentWidth - 50, 300); // Minimum width of 300
				Window.Width = newWidth;
				
				// Update the label with current window size
				windowSizeLabel.Text = $"Window Size: {Window.Width:F0} x {Window.Height:F0}";
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
						Text = "TitleBar Visibility Test",
						AutomationId = "TitleBarAlignmentLabel",
						FontSize = 32,
						HorizontalOptions = LayoutOptions.Center
					},
					windowSizeLabel,
					reduceWidthButton,
				}
		};
	}
}