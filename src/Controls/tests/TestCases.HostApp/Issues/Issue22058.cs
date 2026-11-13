namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22058, "[Windows] OS system components ignore app theme", PlatformAffected.UWP)]
public class Issue22058 : ContentPage
{
	TitleBar customTitleBar;

	public Issue22058()
	{
		this.SetAppThemeColor(BackgroundProperty, Colors.White, Colors.Black);
		customTitleBar = new TitleBar
		{
			Title = "MauiApp1",
			Subtitle = "Welcome to .NET MAUI",
			HeightRequest = 32
		};

		var button = new Button
		{
			Text = "Change To Dark User App Theme",
			AutomationId = "ThemeChangeButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Start,
		};

		button.Clicked += (sender, e) =>
		{
			if (Application.Current is not null)
			{
				Application.Current.UserAppTheme = AppTheme.Dark;
			}
		};

		var resetThemeButton = new Button
		{
			Text = "Reset User App Theme",
			AutomationId = "ResetThemeButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Start,
		};

		resetThemeButton.Clicked += (sender, e) =>
		{
			if (Application.Current is not null)
			{
				Application.Current.UserAppTheme = AppTheme.Unspecified;
			}
		};

		var timePicker = new TimePicker
		{
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "TimePickerControl",
			HorizontalOptions = LayoutOptions.Center,
		};

		var verticalStackLayout = new VerticalStackLayout()
		{
			Spacing = 20,
			Padding = new Thickness(20),
		};

		verticalStackLayout.Add(button);
		verticalStackLayout.Add(resetThemeButton);
		verticalStackLayout.Add(timePicker);

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
