namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33821, "Picker does not respect themes (outline, dropdown, indicator)", PlatformAffected.UWP)]
public partial class Issue33821 : ContentPage
{
	public Issue33821()
	{
		InitializeComponent();
		this.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
	}

	public void OnLightThemeButtonClicked(object sender, EventArgs e)
	{
		if (Application.Current is not null)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			UpdateThemeLabel();
		}
	}

	public void OnDarkThemeButtonClicked(object sender, EventArgs e)
	{
		if (Application.Current is not null)
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
			UpdateThemeLabel();
		}
	}

	void UpdateThemeLabel()
	{
		var themeLabel = this.FindByName<Label>("ThemeLabel");
		if (themeLabel is not null && Application.Current is not null)
		{
			themeLabel.Text = $"Current Theme: {Application.Current.RequestedTheme}";
		}
	}
}
