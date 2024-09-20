namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23921, "Ensure tap is not propagated down when closing SwipeView", PlatformAffected.Android)]
public partial class Issue23921 : ContentPage
{
	public Issue23921()
	{
		InitializeComponent();
	}
}