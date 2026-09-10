using System.Runtime.CompilerServices;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

internal static class AccessibilityExtensions
{
    static readonly ConditionalWeakTable<UICollectionViewCell, AccessibilityTraitState> AccessibilityTraitStates = new();

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

    internal static void UpdateAccessibilityTraits(this UICollectionViewCell cell, ItemsView itemsView, bool invalidateAccessibilityTarget = false)
    {
        var selectionMode = (itemsView as CollectionView)?.SelectionMode;
        var traitState = AccessibilityTraitStates.GetValue(cell, static _ => new AccessibilityTraitState());

        if (selectionMode is null || cell.ContentView is null || cell.ContentView.Subviews.Length == 0)
        {
            ResetAccessibilityTraitState(traitState);
            return;
        }

        var accessibilityRoot = cell.ContentView.Subviews[0];
        if (!ReferenceEquals(traitState.AccessibilityRoot, accessibilityRoot))
        {
            ResetAccessibilityTraitState(traitState);
            traitState.AccessibilityRoot = accessibilityRoot;
        }

        if (selectionMode == SelectionMode.None)
        {
            ClearButtonTrait(traitState);
            return;
        }

        if (!traitState.HasResolvedAccessibilityTarget)
        {
            traitState.AccessibilityTarget = FindAccessibilityElement(accessibilityRoot);
            traitState.HasResolvedAccessibilityTarget = true;
        }

        var accessibilityTarget = traitState.AccessibilityTarget;

        // The exposed accessibility element can change when semantic properties are updated.
        // Always remove the trait from the view where we added it before targeting another view.
        if (!ReferenceEquals(traitState.Target, accessibilityTarget))
        {
            ClearButtonTrait(traitState);
        }

        if (accessibilityTarget is null)
        {
            return;
        }

        // Only track traits added here so developer-assigned Button traits are preserved.
        if ((accessibilityTarget.AccessibilityTraits & UIAccessibilityTrait.Button) == 0)
        {
            accessibilityTarget.AccessibilityTraits |= UIAccessibilityTrait.Button;
            traitState.Target = accessibilityTarget;
            traitState.AddedButtonTrait = true;
        }
    }

    static void ClearButtonTrait(AccessibilityTraitState traitState)
    {
        if (traitState.AddedButtonTrait && traitState.Target is not null)
        {
            traitState.Target.AccessibilityTraits &= ~UIAccessibilityTrait.Button;
        }

        traitState.Target = null;
        traitState.AddedButtonTrait = false;
    }

    static void ResetAccessibilityTraitState(AccessibilityTraitState traitState)
    {
        ClearButtonTrait(traitState);
        traitState.AccessibilityRoot = null;
        traitState.AccessibilityTarget = null;
        traitState.HasResolvedAccessibilityTarget = false;
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

    sealed class AccessibilityTraitState
    {
        public UIView? AccessibilityRoot { get; set; }

        public UIView? AccessibilityTarget { get; set; }

        public bool HasResolvedAccessibilityTarget { get; set; }

        public UIView? Target { get; set; }

        public bool AddedButtonTrait { get; set; }
    }
}
