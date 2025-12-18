namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33171, "When TitleBar.IsVisible = false the caption buttons become unresponsive on Windows", PlatformAffected.UWP)]
public class Issue33171 : ContentPage
{
	TitleBar titleBar;

	public Issue33171()
	{
		Title = "Issue 33171";

		// Create TitleBar
		titleBar = new TitleBar
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
			AutomationId = "WindowSizeLabel",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center
		};

		// Create a button to reduce window width
		var reduceWidthButton = new Button
		{
			Text = "Reduce Window Width",
			AutomationId = "ReduceWidthButton",
		};

		var changeVisibility = new Button
		{
			Text = "Toggle",
			AutomationId = "ToggleTitleBarVisibilityButton",
		};

		var getStatus = new Button
		{
			Text = "Get Status",
			AutomationId = "GetStatusButton",
		};

		getStatus.Clicked += (sender, e) =>
		{
			if (Window != null)
			{
				windowSizeLabel.Text = Window.Width.ToString();
			}
		};

		changeVisibility.Clicked += (sender, e) =>
		{
			if (Window != null && Window.TitleBar != null)
			{
				titleBar.IsVisible = !titleBar.IsVisible;
			}
		};	


		reduceWidthButton.Clicked += (sender, e) =>
		{
			if (Window != null)
			{
				var currentWidth = Window.Width;
				var newWidth = Window.Width/2;
				Window.Width = newWidth;
				Window.Height = Window.Height / 2;
				
				windowSizeLabel.Text = Window.Width.ToString();
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
					changeVisibility,
					getStatus,
					reduceWidthButton,
				}
		};
	}
}