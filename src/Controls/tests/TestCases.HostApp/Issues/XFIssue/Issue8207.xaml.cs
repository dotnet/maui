namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8207, "[Bug] Shell Flyout Items on UWP aren't showing the Title",
	PlatformAffected.UWP)]
public sealed partial class Issue8207 : TestShell
{
	public Issue8207()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
	}
}
