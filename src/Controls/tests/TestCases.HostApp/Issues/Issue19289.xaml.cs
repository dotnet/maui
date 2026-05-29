namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19289, "[Android] Adding a PointerGestureRecognizer to a button on Android stops the button from working", PlatformAffected.Android)]
public partial class Issue19289 : ContentPage
{
	public Issue19289()
	{
		InitializeComponent();
	}

	private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
	{
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		clickedLabel.Text = "Button Works";
	}
}