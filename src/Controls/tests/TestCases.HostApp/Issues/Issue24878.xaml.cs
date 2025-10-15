namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24878, "AppThemeBinding does not work on ToolbarItems", PlatformAffected.Android)]
	public partial class Issue24878 : Shell
	{
		public Issue24878()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
		}
	}
}
