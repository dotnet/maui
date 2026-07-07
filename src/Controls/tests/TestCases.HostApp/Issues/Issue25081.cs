namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25081, "[Windows] The flyout icon and background appear awkward when enabled alongside a TitleBar", PlatformAffected.UWP)]
public class Issue25081 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var content = new Issue25081ContentPage();

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = content
		});
	}
}

public class Issue25081ContentPage : ContentPage
{
	TitleBar customTitleBar;

	public Issue25081ContentPage()
	{
		customTitleBar = new TitleBar
		{
			Title = "MauiApp1",
			Subtitle = "Welcome to .NET MAUI",
			HeightRequest = 32,
			BackgroundColor = Colors.YellowGreen
		};

		var button = new Button
		{
			Text = "Change Background color",
			AutomationId = "ColorChangeButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
		};

		button.Clicked += (sender, e) =>
		{
			customTitleBar.BackgroundColor = Colors.Cyan;
		};

		var verticalStackLayout = new VerticalStackLayout()
		{
			Spacing = 20,
			Padding = new Thickness(20),
		};

		verticalStackLayout.Add(button);

		Content = verticalStackLayout;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (Window is not null)
		{
			Window.TitleBar = customTitleBar;
		}
		else if (Shell.Current?.Window is not null)
		{
			Shell.Current.Window.TitleBar = customTitleBar;
		}
	}
}
