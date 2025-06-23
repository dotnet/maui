namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25581, "Editor Scaling creating new lines on older iOS versions", PlatformAffected.iOS)]
public partial class Issue25581 : ContentPage
{
	public Issue25581()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		editor.Scale = 0.5;
	}
}