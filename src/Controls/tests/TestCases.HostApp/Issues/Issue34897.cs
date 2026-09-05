using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34897, "CollectionView Header is not visible when ItemsSource is not set and EmptyView is set", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34897 : ContentPage
{
	public Issue34897()
	{
		var collectionView = new CollectionView
		{
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
			AutomationId = "Issue34897CollectionView",
			Header = new Label
			{
				Text = "CollectionView Header",
				AutomationId = "Issue34897Header",
				BackgroundColor = Colors.LightBlue,
			},
			EmptyView = new Label
			{
				Text = "No items available",
				AutomationId = "Issue34897EmptyView",
				BackgroundColor = Colors.LightYellow,
			},
			Footer = new Label
			{
				Text = "CollectionView Footer",
				AutomationId = "Issue34897Footer",
				BackgroundColor = Colors.LightGreen,
			},
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			}
		};

		var label = new Label
		{
			Text = "Header, Footer and EmptyView should all be visible below:",
		};

		grid.Add(label, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}
}
