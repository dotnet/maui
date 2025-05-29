namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28180, "Labels with Padding are truncated on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue28180 : ContentPage
{
	public Issue28180()
	{
		InitializeComponent();
	}

	private void OnToggleClicked(object sender, EventArgs e)
	{
		LongTextLabel.Padding = LongTextLabel.Padding == 50 ? 0 : 50;
	}
}