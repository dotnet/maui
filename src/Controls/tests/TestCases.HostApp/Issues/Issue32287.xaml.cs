namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32287, "Using custom TitleView in AppShell causes shell content to be covered in iOS 26", PlatformAffected.iOS)]
public partial class Issue32287 : Shell
{
	public Issue32287()
	{
		InitializeComponent();
	}
}
