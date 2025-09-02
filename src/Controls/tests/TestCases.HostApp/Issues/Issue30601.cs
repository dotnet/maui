namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30601, "[Android] SearchBar does not update colors on theme change", PlatformAffected.Android)]
public class Issue30601 : ContentPage
{
	public Issue30601()
	{
		var searchBar3 = new SearchBar
		{
			Placeholder = "Placeholder with AppThemeBinding - red/green"
		};
		searchBar3.SetAppThemeColor(SearchBar.PlaceholderColorProperty, Colors.Red, Colors.Green);
		this.SetAppThemeColor(BackgroundProperty, Colors.White, Colors.Black);

		Content = new StackLayout
		{
			Children =
				{
					new SearchBar
					{
						Placeholder = "Placeholder - system's default"
					},
					new SearchBar
					{
						PlaceholderColor = Colors.Red,
						Placeholder = "Placeholder - red"
					},
					searchBar3,
					new Button
					{
						Text = "Change Theme",
						AutomationId = "changeThemeButton",
						Command= new Command(() =>
						{
							Application.Current!.UserAppTheme = Application.Current!.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
						})
					}
				}
		};
	}
}