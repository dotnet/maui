namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24034, "Shadow is not updating on change of parent control", PlatformAffected.All)]
public partial class Issue24034 : ContentPage
{
	public Issue24034()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		GridView.IsVisible = !GridView.IsVisible;
	}
}