namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22058, "[Windows] OS system components ignore app theme", PlatformAffected.UWP)]
public class Issue22058 : ContentPage
{
	public Issue22058()
	{
		var button = new Button
		{
			Text = "Change User App Theme",
			AutomationId = "ThemeChangeButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		button.Clicked += (sender, e) =>
		{
			if(Application.Current is not null)
			{
				if (Application.Current.UserAppTheme == AppTheme.Dark)
				{
					Application.Current.UserAppTheme = AppTheme.Unspecified;
				}
				else
				{
					Application.Current.UserAppTheme = AppTheme.Dark;
				}
			}
		};

		var timePicker = new TimePicker
		{
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "timePicker",
			HorizontalOptions = LayoutOptions.End,
		};

		var verticalStackLayout = new VerticalStackLayout()
		{
			Spacing = 20,
			Padding = new Thickness(20),
		};

		verticalStackLayout.Add(button);
		verticalStackLayout.Add(timePicker);

		Content = verticalStackLayout;
	}
}
