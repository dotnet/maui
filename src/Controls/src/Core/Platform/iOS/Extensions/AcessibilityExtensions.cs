using System.Runtime.CompilerServices;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AcessibilityExtensions
{
	// Defensive cap on recursion depth, not a UIKit limit; real item templates rarely nest this deep.
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
		if (cell.ContentView is null
			|| cell.ContentView.Subviews.Length == 0
			|| selectionMode is null)
		{
			return;
		}

		if (selectionMode == SelectionMode.None)
		{
			// Nothing to apply. Only clear a previously-applied trait (e.g. the cell is being
			// reused after a prior bind with Single/Multiple selection); avoid re-running the
			// accessibility-element search when this cell never had the trait applied.
			if (s_appliedAccessibilityElements.TryGetValue(cell, out var previouslyAppliedElement))
			{
				previouslyAppliedElement.AccessibilityTraits &= ~UIAccessibilityTrait.Button;
				s_appliedAccessibilityElements.Remove(cell);
			}

			return;
		}

		var firstChild = cell.ContentView.Subviews[0];

		// if the first child is a control, changing the accessibility traits from an entry to a button could be confusing.
		if (firstChild is UIControl)
		{
			return;
		}

		var accessibilityElement = FindAccessibilityElement(firstChild, 0) ?? firstChild;

		accessibilityElement.AccessibilityTraits |= UIAccessibilityTrait.Button;
		s_appliedAccessibilityElements.Remove(cell);
		s_appliedAccessibilityElements.Add(cell, accessibilityElement);
	}

	// Tracks which descendant view (if any) last received the Button trait for a given cell,
	// so that switching to SelectionMode.None can clear the trait from that exact element
	// without re-running the recursive accessibility-element search on every bind/rebind.
	static readonly ConditionalWeakTable<UICollectionViewCell, UIView> s_appliedAccessibilityElements = new();

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
