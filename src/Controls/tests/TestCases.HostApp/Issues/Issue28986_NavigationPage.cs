namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea Navigation Page for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 7)]
public partial class Issue28986_NavigationPage : NavigationPage
{
	public Issue28986_NavigationPage() : base(new Issue28986_ContentPage())
	{
        BarBackground = Colors.Blue;
	}
}