using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AcessibilityExtensions
{
	const int MaxAccessibilityElementDepth = 10;

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

			var accessibilityElement = FindAccessibilityElement(firstChild, 0) ?? firstChild;

			if (selectionMode != SelectionMode.None)
			{
				accessibilityElement.AccessibilityTraits |= UIAccessibilityTrait.Button;
			}
			else
			{
				accessibilityElement.AccessibilityTraits &= ~UIAccessibilityTrait.Button;
			}
		}
	}

	static UIView? FindAccessibilityElement(UIView view, int depth)
	{
		if (view is UIControl)
		{
			return null;
		}

		if (view.IsAccessibilityElement)
		{
			return view;
		}

		if (depth >= MaxAccessibilityElementDepth)
		{
			return null;
		}

		var subviews = view.Subviews;
		for (var index = 0; index < subviews.Length; index++)
		{
			if (FindAccessibilityElement(subviews[index], depth + 1) is UIView accessibilityElement)
			{
				return accessibilityElement;
			}
		}

		return null;
	}
}
