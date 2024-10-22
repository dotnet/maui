namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22937, "ToolbarItem font color not updating properly after changing the available state at runtime", PlatformAffected.Android)]
public partial class Issue22937 : Shell
{

	public Issue22937()
	{
		InitializeComponent();
	}

	private void ChangeStateClicked(object sender, EventArgs e)
	{
		EditButton.IsEnabled = !EditButton.IsEnabled;
	}
}