namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25233, "CollectionView with SwipeView items behaves strangely", PlatformAffected.All)]

public partial class Issue25233 : ContentPage
{
	public Issue25233()
	{
		InitializeComponent();
		cv.ItemsSource = Enumerable.Range(1, 15).Select(i => $"Item{i}");
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		cv.ScrollTo(14, position: ScrollToPosition.End, animate: false);
	}
}
