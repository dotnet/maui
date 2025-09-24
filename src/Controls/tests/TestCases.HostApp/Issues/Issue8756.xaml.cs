namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8756, "3 dots in menu is in black in dark theme in Windows 10, but white in Windows 11", PlatformAffected.Windows)]
public partial class Issue8756 : ContentPage
{
	public Issue8756()
	{
		InitializeComponent();
	}

	void OnOpenMenuClicked(object sender, EventArgs e)
	{
		// This is just for testing - the actual issue is with the toolbar overflow appearance
		DisplayAlert("Menu", "Click the 3-dot overflow menu to see the theming issue", "OK");
	}
}