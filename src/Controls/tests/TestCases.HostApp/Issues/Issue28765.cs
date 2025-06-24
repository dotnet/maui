namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28765, "[Android] Inconsistent footer scrolling in CollectionView when EmptyView as string", PlatformAffected.Android)]
public class Issue28765 : TestContentPage
{
	override protected void Init()
	{
		Grid grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		CollectionView headerFooterView = new CollectionView
		{
			EmptyView = "No items to display",
			Header = new Label
			{
				Text = "Header View",
			},
			Footer = new Label
			{
				Text = "Footer View"
			}
		};

		CollectionView headerFooterStringView = new CollectionView
		{
			EmptyView = "No items to display",
			Header = "Header String",
			Footer = "Footer String"
		};
		Grid.SetRow(headerFooterStringView, 1);

		grid.Children.Add(headerFooterView);
		grid.Children.Add(headerFooterStringView);

		Content = grid;
	}
}