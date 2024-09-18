namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 15524, "Text entry border disappear when changing to/from dark mode in Android", PlatformAffected.Android)]
public partial class Issue15524 : ContentPage
{
	public Issue15524()
	{
		InitializeComponent();
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		if (Application.Current.RequestedTheme == AppTheme.Dark)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
		}
		else
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
		}
	}
}