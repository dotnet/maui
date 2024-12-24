namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7453, "ShellContent Title doesn't observe changes to bound properties", PlatformAffected.UWP | PlatformAffected.Android)]
public partial class Issue7453 : Shell
{
	public Issue7453()
	{
		InitializeComponent();
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		this.tab.Title = "Updated title";
	}
}