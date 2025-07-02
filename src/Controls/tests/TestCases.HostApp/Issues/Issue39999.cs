namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39999, "RTL FlowDirection causes overlap with window control buttons", PlatformAffected.UWP)]
public class Issue39999 : ContentPage
{
	TitleBar titleBar;
	Button toggleButton;
	
	public Issue39999()
	{
		// Create TitleBar similar to MainPage.xaml
		titleBar = new TitleBar
		{
			Title = "Maui App",
			Subtitle = "Hello, World!",
			ForegroundColor = Colors.Red,
			HeightRequest = 48
		};
		
		// Add leading content (image)
		titleBar.LeadingContent = new Image
		{
			Source = "dotnet_bot.png",
			HeightRequest = 24,
			VerticalOptions = LayoutOptions.Center
		};
		
		// Add main content (search bar)
		titleBar.Content = new SearchBar
		{
			Placeholder = "Search",
			PlaceholderColor = Colors.White,
			BackgroundColor = Colors.LightGray,
			MaximumWidthRequest = 300,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Center
		};
		
		// Add trailing content (image button)
		titleBar.TrailingContent = new ImageButton
		{
			Source = "dotnet_bot.png",
			BackgroundColor = Colors.Yellow,
			CornerRadius = 50,
			HeightRequest = 36,
			WidthRequest = 36
		};
		
		// Create toggle button
		toggleButton = new Button
		{
			HeightRequest = 50,
			WidthRequest = 300,
			Text = "Toggle TitleBar FlowDirection",
			AutomationId = "ToggleFlowDirectionButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
		};

		this.Loaded += (s, e) =>
		{
			if (Window != null)
			{
				Window.TitleBar = titleBar;
			}
		};
		
		// Add click handler for the button
		toggleButton.Clicked += OnToggleFlowDirectionClicked;
		
		// Set up the content layout with button centered vertically
		Content = new Grid
		{
			VerticalOptions = LayoutOptions.Fill,
			HorizontalOptions = LayoutOptions.Fill,
			Children = { toggleButton }
		};
	}
	
	void OnToggleFlowDirectionClicked(object sender, EventArgs e)
	{
		if (titleBar.FlowDirection == FlowDirection.LeftToRight)
		{
			titleBar.FlowDirection = FlowDirection.RightToLeft;
		}
		else
		{
			titleBar.FlowDirection = FlowDirection.LeftToRight;
		}
	}
}