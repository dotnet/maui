namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 19997, "[Android, iOS, MacOS]Entry ClearButton Color Not Updating on AppThemeBinding Change", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue19997 : ContentPage
{
	public Issue19997()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{

		if (Application.Current != null)
		{
			Application.Current!.UserAppTheme = Application.Current!.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
		}
	}
}