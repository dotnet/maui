namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19313, "Checkbox broken in Android", PlatformAffected.Android)]
public partial class Issue19313 : ContentPage
{
	public Issue19313()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		tappedLabel.Text = "Tapped";
	}

	private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		checkedLabel.Text = "Checked";
	}
}