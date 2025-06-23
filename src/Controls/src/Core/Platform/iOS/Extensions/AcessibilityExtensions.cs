using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AcessibilityExtensions
{
	internal static void UpdateAccessibilityTraits(this UICollectionView collectionView, SelectableItemsView itemsView)
	{
		foreach (var subview in collectionView.Subviews)
		{
			if (subview is UICollectionViewCell cell)
			{
				cell.UpdateAccessibilityTraits(itemsView);
			}
		}
	}

	internal static void UpdateAccessibilityTraits(this UICollectionViewCell cell, ItemsView itemsView)
	{
		var selectionMode = (itemsView as CollectionView)?.SelectionMode;
		if (cell.ContentView is not null
			&& cell.ContentView.Subviews.Length > 0
			&& selectionMode is not null)
		{
			var firstChild = cell.ContentView.Subviews[0];

			// if the first child is a control, changing the accessibility traits from an entry to a button could be confusing.
			if (firstChild is UIControl)
			{
				return;
			}

			if (selectionMode != SelectionMode.None)
			{
				firstChild.AccessibilityTraits |= UIAccessibilityTrait.Button;
			}
			else
			{
				firstChild.AccessibilityTraits &= ~UIAccessibilityTrait.Button;
			}
		}
	}
}
