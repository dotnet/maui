namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24627, "[Windows] TitleBar Title FontAttributes", PlatformAffected.UWP)]
public class Issue24627 : ContentPage
{
	TitleBar customTitleBar;

	public Issue24627()
	{
		customTitleBar = new TitleBar
		{
			Title = "MauiApp1",
			Subtitle = "Welcome to .NET MAUI",
			TitleFontAttributes = FontAttributes.Bold,
			HeightRequest = 32
		};

		var label = new Label
		{
			Text = "Welcome to .NET MAUI",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var button = new Button
		{
			Text = "Set Title FontAttributes to None",
			AutomationId = "ChangeFAButton",
			Command = new Command(() =>
			{
				customTitleBar.TitleFontAttributes = FontAttributes.None;
			}),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var verticalStack = new VerticalStackLayout
		{
			Children =
			{
				label,
				button
			},
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center
		};

		Content = verticalStack;
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
