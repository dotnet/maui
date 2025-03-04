namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25747, "Updating a Border.TranslationY does not work", PlatformAffected.Android)]

public partial class Issue25747 : ContentPage
{
	public Issue25747()
	{
		InitializeComponent();
	}

	bool _goDown = true;
	void Button_Clicked(object sender, EventArgs e)
	{
		Border.TranslationY = _goDown ? 100 : 0;
		_goDown = !_goDown;
		Border.InputTransparent = !Border.InputTransparent;
	}
}
