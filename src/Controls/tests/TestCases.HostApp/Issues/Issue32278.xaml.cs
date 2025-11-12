namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32278, "Shell content page title position incorrect/clipped", PlatformAffected.Android)]
public partial class Issue32278 : Shell
{
	public Issue32278()
	{
		InitializeComponent();
		Routing.RegisterRoute("NewPage1", typeof(Issue32278NewPage));
	}
}

public class Issue32278MainPage : ContentPage
{
	public Issue32278MainPage()
	{
		Title = "MainPage";
		
		var button = new Button
		{
			Text = "Navigate to NewPage1",
			AutomationId = "NavigateButton"
		};
		
		button.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("NewPage1");
		};
		
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Label { Text = "Welcome to .NET MAUI!" },
				button
			}
		};
	}
}

public class Issue32278NewPage : ContentPage
{
	public Issue32278NewPage()
	{
		Title = "NewPage1";
		
		// Add a label at the top to help verify that content is positioned correctly
		// If SafeAreaEdges is not working, content will be clipped under the toolbar
		var topLabel = new Label
		{
			Text = "Top Label - Should be visible",
			AutomationId = "TopLabel",
			BackgroundColor = Colors.Yellow,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start
		};
		
		var contentLabel = new Label
		{
			Text = "Welcome to .NET MAUI!",
			AutomationId = "ContentLabel",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};
		
		Content = new Grid
		{
			BackgroundColor = Colors.White,
			Children =
			{
				new VerticalStackLayout
				{
					Spacing = 10,
					Children =
					{
						topLabel,
						contentLabel
					}
				}
			}
		};
	}
}
