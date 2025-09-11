namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30603, "[Android] Editor and Entry don't update placeholder and text color on theme change", PlatformAffected.Android)]
public class Issue30603 : ContentPage
{
	public Issue30603()
	{
		this.SetAppThemeColor(BackgroundProperty, Colors.White, Colors.Black);

		Content = new StackLayout
		{
			Children =
				{
					new Editor
					{
						Placeholder = "Editor - Placeholder - system's default"
					},
					new Entry
					{
						Placeholder = "Entry - Placeholder - system's default"
					},
					new Editor
					{
						Placeholder = "Editor - Text - system's default"
					},
					new Entry
					{
						Placeholder = "Entry - Text - system's default"
					},
					new Button
					{
						Text = "Change Theme",
						AutomationId = "changeThemeButton",
						Command = new Command(() =>
						{
							Application.Current!.UserAppTheme = Application.Current!.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
						})
					}
				}
		};
	}
}