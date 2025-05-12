namespace Controls.TestCases.HostApp.Issues;
[Issue(IssueTracker.Github, 29411, "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime", PlatformAffected.All)]
public partial class Issue29411 : ContentPage
{
	public Issue29411()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		cview.Loop = !cview.Loop;
	}
}