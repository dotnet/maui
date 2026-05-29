namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 9978, "VisualElement.HeightRequest defaults to 0 instead of -1 when using OnIdiom default value",
		PlatformAffected.All)]
public partial class Issue9978 : ContentPage
{
	public Issue9978()
	{
		InitializeComponent();
	}
}