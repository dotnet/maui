namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29559, "Add An API to SearchHandler so users can hide or show the softkeyboard", PlatformAffected.Android | PlatformAffected.iOS)]
public partial class Issue29559 : Shell
{
	public Issue29559()
	{
		InitializeComponent();
	}

	private void ShowKeyboardClicked(object sender, EventArgs e)
		=> searchHandler.ShowSoftInputAsync();

	private void HideKeyboardClicked(object sender, EventArgs e)
		=> searchHandler.HideSoftInputAsync();
}