namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10660, "Inconsistent toolbar text color on interaction", PlatformAffected.Android)]
public partial class Issue10660 : Shell
{

	public Issue10660()
	{
		InitializeComponent();
	}

	private void ChangeTextClicked(object sender, EventArgs e)
	{
		StateButton.Text = StateButton.Text == "Close" ? "Open" : "Close";
	}
}