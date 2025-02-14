namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27377, "SwipeView: SwipeItem.IconImageSource.FontImageSource color value not honored", PlatformAffected.iOS)]
public partial class Issue27377 : ContentPage
{
	public Issue27377()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		swipeView.Open(OpenSwipeItem.RightItems, true);
	}
}