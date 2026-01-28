namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16624, "SwipeGesture is not working on a ListView/CollectionView", PlatformAffected.Android)]
public partial class Issue16624 : ContentPage
{
	public Issue16624()
	{
		InitializeComponent();
	}

	private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
	{
		StatusLabel.Text = $"Swiped {e.Direction}";
	}
}
