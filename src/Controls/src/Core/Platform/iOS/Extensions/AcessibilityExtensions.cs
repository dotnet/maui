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

        if (selectionMode is null || cell.ContentView is null || cell.ContentView.Subviews.Length == 0)
        {
            return;
        }

        var accessibilityTarget = FindAccessibilityElement(cell.ContentView.Subviews[0]);

        // Preserve the accessibility role of native controls.
        if (accessibilityTarget is null || accessibilityTarget is UIControl)
        {
            return;
        }

        if (selectionMode != SelectionMode.None)
        {
            accessibilityTarget.AccessibilityTraits |= UIAccessibilityTrait.Button;
        }
        else
        {
            accessibilityTarget.AccessibilityTraits &= ~UIAccessibilityTrait.Button;
        }
    }

    static UIView? FindAccessibilityElement(UIView view)
    {
        // Do not change the role of native controls such as
        // UIButton, UITextField, UISwitch, etc.
        if (view is UIControl)
        {
            return null;
        }

        // This is the native view that MAUI has exposed to
        // VoiceOver as the accessibility element.
        if (view.IsAccessibilityElement)
        {
            return view;
        }

        foreach (var subview in view.Subviews)
        {
            var accessibilityElement = FindAccessibilityElement(subview);

            if (accessibilityElement is not null)
            {
                return accessibilityElement;
            }
        }

        return null;
    }
}
