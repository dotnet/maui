using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AcessibilityExtensions
{
	internal static void UpdateAccessibilityTraits(this UICollectionView collectionView, SelectableItemsView itemsView)
	{
		foreach (var subview in collectionView.Subviews)
		{
			if (subview is UICollectionViewCell cell && cell.ContentView is not null && cell.ContentView.Subviews.Length > 0)
			{
				var firstChild = cell.ContentView.Subviews[0];

				if (itemsView.SelectionMode != SelectionMode.None)
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

	internal static void UpdateAccessibilityTraits(this UICollectionViewCell cvCell, ItemsView itemsView)
	{
		var selectionMode = (itemsView as CollectionView)?.SelectionMode;
		if (cvCell is UICollectionViewCell cell
			&& cell.ContentView is not null
			&& cell.ContentView.Subviews.Length > 0
			&& selectionMode is not null)
		{
			var firstChild = cell.ContentView.Subviews[0];

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
