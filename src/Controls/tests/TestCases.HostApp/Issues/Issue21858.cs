namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21858, "DatePicker not respecting UserAppTheme", PlatformAffected.Android)]
	public class Issue21858 : ContentPage
	{
		public Issue21858()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new DatePicker() {
						AutomationId = "DatePicker",
						Format = "D",
						Date = DateTime.Now
					},
					new Button()
					{
						Text = "Toggle Theme",
						AutomationId = "ToggleThemeButton",
						Command = new Command(() => Application.Current!.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark)
					}
				},
			};
		}
	}
}