namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31889, "Entry and Editor AppThemeBinding colors for text and placeholder reset to default on theme change", PlatformAffected.Android)]
public partial class Issue31889 : ContentPage
{
	public Issue31889()
	{
		InitializeComponent();
	}
	public void OnLightThemeButtonClicked(object sender, EventArgs e)
	{
		var app = Application.Current;
		if (app != null)
			app.UserAppTheme = AppTheme.Light;
	}

	public void OnDarkThemeButtonClicked(object sender, EventArgs e)
	{
		var app = Application.Current;
		if (app != null)
			app.UserAppTheme = AppTheme.Dark;
	}
}