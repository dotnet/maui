using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AcessibilityExtensions
{
	internal static void UpdateSelectedAccessibility(this UICollectionViewCell cell, bool selected)
	{
		// Catalyst applies/removes the selected trait to the cell automatically.
		// iOS does not and the VoiceOver on iOS only announces when the selected trait is applied to the first child of the cell.
#if IOS
		if (cell.ContentView is not null && cell.ContentView.Subviews.Length > 0)
		{
			var firstChild = cell.ContentView.Subviews[0];

			if (selected)
			{
				firstChild.AccessibilityTraits |= UIAccessibilityTrait.Selected;
			}
			else
			{
				firstChild.AccessibilityTraits &= ~UIAccessibilityTrait.Selected;
			}
		}
#endif
	}

	internal static void ClearSelectedAccessibilityTraits(this UICollectionView collectionView, Foundation.NSIndexPath[] indices)
	{
		// Catalyst applies/removes the selected trait to the cell automatically.
		// iOS does not and the VoiceOver on iOS only announces when the selected trait is applied to the first child of the cell.
#if IOS
		foreach (var index in indices)
		{
			var cell = collectionView.CellForItem(index);
			if (cell?.ContentView is not null && cell.ContentView.Subviews.Length > 0)
			{
				var firstChild = cell.ContentView.Subviews[0];
				firstChild.AccessibilityTraits &= ~UIAccessibilityTrait.Selected;
			}
		}
#endif
	}

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
