namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32278, "Shell content page title position incorrect/clipped", PlatformAffected.Android)]
public partial class Issue32278 : Shell
{
	public Issue32278()
	{
		InitializeComponent();
	}
}

public class Issue32278MainPage : ContentPage
{
	public Issue32278MainPage()
	{
		Title = "MainPage";
		
		// Add a top label on the main page to compare position with the second page
		var topLabel = new Label
		{
			Text = "Top Label - Page 1",
			AutomationId = "TopLabelPage1",
			BackgroundColor = Colors.LightBlue,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start,
			Padding = 10
		};
		
		var button = new Button
		{
			Text = "Navigate to Page 2",
			AutomationId = "NavigateButton"
		};
		
		button.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new Issue32278NewPage());
		};
		
		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
			{
				topLabel,
				button
			}
		};
	}
}

public class Issue32278NewPage : ContentPage
{
	public Issue32278NewPage()
	{
		Title = "Page 2";
		
		// Add a label at the top to help verify that content is positioned correctly
		// If SafeAreaEdges is not working, content will be clipped under the toolbar
		var topLabel = new Label
		{
			Text = "Top Label - Page 2",
			AutomationId = "TopLabelPage2",
			BackgroundColor = Colors.Yellow,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start,
			Padding = 10
		};
		
		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
			{
				topLabel
			}
		};
	}
}
