namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20156, "Border has a 1 pixel thickness even when it's thickness property is set to 0", PlatformAffected.All)]

public partial class Issue20156 : ContentPage
{
	public Issue20156()
	{
		InitializeComponent();
	}
}
