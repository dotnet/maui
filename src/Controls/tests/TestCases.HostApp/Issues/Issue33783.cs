namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33783, "Switch ThumbColor not applied correctly when theme changes on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue33783 : ContentPage
{
	public Issue33783()
	{
		var testSwitch = new Switch
		{
			AutomationId = "TestSwitch",
			OnColor = Colors.Green,
			ThumbColor = Colors.Red,
			IsToggled = true,
			HorizontalOptions = LayoutOptions.Center
		};

		var lightThemeButton = new Button
		{
			AutomationId = "LightThemeButton",
			Text = "Light Theme"
		};

		lightThemeButton.Clicked += (s, e) =>
		{
			if (Application.Current != null)
			{
				Application.Current.UserAppTheme = AppTheme.Light;
			}
		};

		var darkThemeButton = new Button
		{
			AutomationId = "DarkThemeButton",
			Text = "Dark Theme"
		};

		darkThemeButton.Clicked += (s, e) =>
		{
			if (Application.Current != null)
			{
				Application.Current.UserAppTheme = AppTheme.Dark;
			}
		};

		var themeButtonsLayout = new HorizontalStackLayout
		{
			Spacing = 10,
			HorizontalOptions = LayoutOptions.Center,
			Children = { lightThemeButton, darkThemeButton }
		};

		var contentLayout = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30),
			Children = { testSwitch, themeButtonsLayout }
		};
		contentLayout.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
		Content = contentLayout;
	}
}