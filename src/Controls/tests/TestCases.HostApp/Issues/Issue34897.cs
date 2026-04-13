using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34897, "CollectionView Header is not visible when ItemsSource is not set and EmptyView is set", PlatformAffected.iOS | PlatformAffected.MacCatalyst)]
public class Issue34897 : ContentPage
{
	public Issue34897()
	{
		// This reproduces the bug: Header set statically with EmptyView active and no ItemsSource.
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
			EmptyView = new Label
			{
				Text = "No items available",
				AutomationId = "Issue34897EmptyView",
				BackgroundColor = Colors.LightYellow,
				Padding = new Thickness(8),
			},
			// ItemsSource intentionally NOT set (null) — this is the bug scenario
		};

		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Header and EmptyView should both be visible below:",
					Margin = new Thickness(8),
				},
				collectionView,
			}
		};
	}
}
