namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34911, "[Android] StatusBar color doesn't update immediately when changing UserAppTheme in Shell", PlatformAffected.Android)]
public class Issue34911 : Shell
{
	public Issue34911()
	{
		// Reset to light theme for a deterministic start state
		if (Application.Current != null)
			Application.Current.UserAppTheme = AppTheme.Light;

		FlyoutBehavior = FlyoutBehavior.Flyout;

		// Home page — theme toggle button lives here
		Items.Add(new FlyoutItem
		{
			Title = "Home",
			Items =
			{
				new ShellContent
				{
					Title = "Home",
					Content = new Issue34911HomePage()
				}
			}
		});

		// Settings page — second flyout item (mirrors sandbox structure)
		Items.Add(new FlyoutItem
		{
			Title = "Settings",
			Items =
			{
				new ShellContent
				{
					Title = "Settings",
					Content = new Issue34911SettingsPage()
				}
			}
		});
	}
}

public class Issue34911HomePage : ContentPage
{
	public Issue34911HomePage()
	{
		Title = "Home";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 16,
			Children =
			{
				new Label
				{
					Text = "StatusBar should be Blue (Light) or Black (Dark) immediately after toggling.",
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Button
				{
					Text = "Toggle Theme",
					AutomationId = "ToggleThemeButton",
					Command = new Command(() =>
					{
						if (Application.Current != null)
						{
							Application.Current.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark
								? AppTheme.Dark
								: AppTheme.Light;
						}
					})
				}
			}
		};
	}
}

public class Issue34911SettingsPage : ContentPage
{
	public Issue34911SettingsPage()
	{
		Title = "Settings";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Settings Page",
					AutomationId = "SettingsPageLabel"
				}
			}
		};
	}
}
