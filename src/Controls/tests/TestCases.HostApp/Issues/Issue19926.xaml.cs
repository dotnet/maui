namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19926, "[Android] Opacity bug on BoxView.Background", PlatformAffected.Android)]

public partial class Issue19926 : ContentPage
{
	public Issue19926()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, System.EventArgs e)
	{
		boxView.IsVisible = true;
	}
}
