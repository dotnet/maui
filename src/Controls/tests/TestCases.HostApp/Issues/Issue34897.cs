using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34897, "CollectionView Header is not visible when ItemsSource is not set and EmptyView is set", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34897 : ContentPage
{
	public Issue34897()
	{
		// This reproduces the bug: Header set statically with EmptyView active and ItemsSource=null.
		// UICollectionViewCompositionalLayout drops boundary supplementary items (header/footer)
		// when NumberOfSections returns 0 — which happens when ItemsSource is null.
		var collectionView = new CollectionView
		{
			AutomationId = "Issue34897CollectionView",
			Header = new Label
			{
				Text = "CollectionView Header",
				AutomationId = "Issue34897Header",
				BackgroundColor = Colors.LightBlue,
				Padding = new Thickness(8),
			},
			Footer = new Label
			{
				Text = "CollectionView Footer",
				AutomationId = "Issue34897Footer",
				BackgroundColor = Colors.LightGreen,
				Padding = new Thickness(8),
			},
			EmptyView = new Label
			{
				Text = "No items available",
				AutomationId = "Issue34897EmptyView",
				BackgroundColor = Colors.LightYellow,
				Padding = new Thickness(8),
			},
			// ItemsSource intentionally set to null (not an empty collection)
			// This is the key condition that triggers the bug
			ItemsSource = null,
		};

		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Header, Footer and EmptyView should all be visible below:",
					Margin = new Thickness(8),
				},
				collectionView,
			}
		};
	}
}
