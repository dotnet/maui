namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19752, "Pointer-over visual state has higher priority than focused state", PlatformAffected.UWP)]
public partial class Issue19752 : ContentPage
{
	public Issue19752()
	{
		InitializeComponent();
	}

	private void button1_Clicked(object sender, EventArgs e)
	{
		button1.Focus();
	}
}