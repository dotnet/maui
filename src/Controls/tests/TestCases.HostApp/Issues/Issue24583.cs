namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24583, "Text in the Editor control disappeared when reducing the Scale", PlatformAffected.iOS)]

public partial class Issue24583 : ContentPage
{
	public Issue24583()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		editor1.Scale = editor2.Scale = editor3.Scale = 0.5;
	}
}
