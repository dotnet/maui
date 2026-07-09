namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36322, "Shell.TitleView is not centered on initial load on Windows", PlatformAffected.UWP)]
public partial class Issue36322 : Shell
{
	public Issue36322()
	{
		InitializeComponent();
	}
}
