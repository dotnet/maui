namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27992, "Entry Completed Event Triggered Twice", PlatformAffected.Android)]
public partial class Issue27992 : ContentPage
{
	public Issue27992()
	{
		InitializeComponent();
	}
	int i = 0;
	private void Entry_Completed(object sender, EventArgs e)
	{
		i++;
		label.Text = $"Completed Invoked {i} times";
	}
}