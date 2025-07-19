namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28968, "[iOS] [ActivityIndicator] IsRunning ignores IsVisible when set to true",PlatformAffected.iOS)]
public partial class Issue28968 : ContentPage
{
	public Issue28968()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		activityIndicator.IsRunning = true;
	}
}