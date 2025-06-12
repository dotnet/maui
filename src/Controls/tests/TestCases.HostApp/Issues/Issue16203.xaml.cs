namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16203, "SwipeGestureRecognizer does not raise Swiped event", PlatformAffected.Android)]
public partial class Issue16203 : ContentPage
{
	public Issue16203()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		clickedLabel.Text = "Clicked";
	}

	private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
	{
		swipedLabel.Text = "Swiped";
	}
}