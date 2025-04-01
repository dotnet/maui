namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22289, "InputTransparent=\"true\" on a Layout breaks child controls on Android", PlatformAffected.Android)]

public partial class Issue22289 : ContentPage
{
	public Issue22289()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, System.EventArgs e)
	{
		Button1.IsVisible = !Button1.IsVisible;
		Button2.IsVisible = !Button2.IsVisible;
	}
}
