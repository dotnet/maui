namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28163, "RadioButton Focused and Unfocused Events Not Firing on iOS and Android", PlatformAffected.iOS | PlatformAffected.Android)]
public partial class Issue28163 : ContentPage
{
	public Issue28163()
	{
		InitializeComponent();
	}
	private void RadioButton_Focused(object sender, FocusEventArgs e)
	{
		focusedlabel.Text = "True";
	}

	private void RadioButton_Unfocused(object sender, FocusEventArgs e)
	{
		unfocusedlabel.Text = "True";
	}

	private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		checkChangedlabel.Text = "True";
	}
}