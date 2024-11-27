namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12714,
		"[Bug] iOS application suspended at UICollectionViewFlowLayout.PrepareLayout() when using IsVisible = false",
		PlatformAffected.iOS)]
	public class Issue12714 : TestContentPage
	{
		const string Success = "Success";
		const string Show = "Show";

		protected override void Init()
		{
			var items = new List<string>() { "uno", "dos", "tres", Success };
			var cv = new CollectionView
			{
				ItemsSource = items,
				IsVisible = false,
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			};

			var layout = new StackLayout() { Margin = 40 };

			var button = new Button { AutomationId = Show, Text = Show };
			button.Clicked += (sender, args) => { cv.IsVisible = !cv.IsVisible; };

			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}
	}
}
