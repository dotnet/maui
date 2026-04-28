namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 29391, "[iOS] IsSwipeEnabled Not Working on CarouselView (CV2)", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue29391 : ContentPage
	{
		public Issue29391()
		{
			InitializeComponent();
		}

		void OnScrollToItem3Clicked(object sender, EventArgs e)
		{
			CarouselView.ScrollTo(2, animate: false);
		}

		void OnPositionChanged(object sender, PositionChangedEventArgs e)
		{
			PositionLabel.Text = $"Position: {e.CurrentPosition}";
		}
	}
}
