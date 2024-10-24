namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25474, "CollectionView can't scroll to the last item when TabBar is visible", PlatformAffected.iOS)]
	public partial class Issue25474 : Shell
	{
		public Issue25474()
		{
			InitializeComponent();
			collectionView.ItemsSource = Enumerable.Range(1, 10).Select(i => $"Item{i}");
		}

		protected void Button_Clicked(object sender, EventArgs e)
		{
			collectionView.ScrollTo(9, position: ScrollToPosition.End, animate: false);
		}
	}
}