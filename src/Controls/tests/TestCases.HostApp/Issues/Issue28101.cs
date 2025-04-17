using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28101, "CollectionView Footer Becomes Scrollable When EmptyView is Active on Android", PlatformAffected.Android)]
	public class Issue28101 : TestContentPage
	{
		protected override void Init()
		{
			Grid grid = new Grid();
			CollectionView collectionView = new CollectionView
			{
				EmptyView = new Label
				{
					Padding = new Thickness(20, 5, 5, 5),
					Text = "Empty"
				}
			};

			collectionView.HeaderTemplate = new DataTemplate(() =>
			{
				Grid gridHeader = new Grid();

				Label labelHeader = new Label
				{
					FontSize = 20,
					BackgroundColor = Colors.HotPink,
					Text = "This Is A Header"
				};

				gridHeader.Children.Add(labelHeader);
				return gridHeader;
			});

			collectionView.FooterTemplate = new DataTemplate(() =>
			{
				Grid gridFooter = new Grid();

				Label labelFooter = new Label
				{
					Text = "This Is A Footer",
					BackgroundColor = Colors.HotPink,
					FontSize = 20,
				};

				gridFooter.Children.Add(labelFooter);
				return gridFooter;
			});

			grid.Children.Add(collectionView);
			Content = grid;
		}
	}
}
