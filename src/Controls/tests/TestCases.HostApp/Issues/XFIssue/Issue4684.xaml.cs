namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4684, "[Android] don't clear shell content because native page isn't visible",
	PlatformAffected.Android)]
public sealed partial class Issue4684 : TestShell
{
	public Issue4684()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
	}
}
