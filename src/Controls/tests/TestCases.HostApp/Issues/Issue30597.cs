namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30597, "[iOS] SearchBar placeholder color is not updating on theme change", PlatformAffected.iOS)]
public class Issue30597 : ContentPage
{
	public Issue30597()
	{
		var searchBar3 = new SearchBar
		{
			Placeholder = "Placeholder with AppThemeBinding - red/green"
		};
		searchBar3.SetAppThemeColor(SearchBar.PlaceholderColorProperty, Colors.Red, Colors.Green);

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