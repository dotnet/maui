namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32526, "Shell content page title position incorrect/clipped", PlatformAffected.Android)]
public partial class Issue32526 : Shell
{
	public Issue32526()
	{
		InitializeComponent();
	}
}

public class Issue32526MainPage : ContentPage
{
	public Issue32526MainPage()
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
			await Navigation.PushAsync(new Issue32526NewPage());
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

public class Issue32526NewPage : ContentPage
{
	public Issue32526NewPage()
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
