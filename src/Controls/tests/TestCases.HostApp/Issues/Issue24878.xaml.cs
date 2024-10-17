namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24878, "AppThemeBinding does not work on ToolbarItems", PlatformAffected.Android)]
	public partial class Issue24878 : Shell
	{
		public Issue24878()
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
		}
	}
}
