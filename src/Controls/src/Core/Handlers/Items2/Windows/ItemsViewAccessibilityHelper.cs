using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
namespace Microsoft.Maui.Controls.Handlers.Items2;

sealed class ItemsViewAccessibilityHelper
{
    readonly MauiItemsView _itemsView;
    private bool _redirectingFocus;

    public ItemsViewAccessibilityHelper(MauiItemsView itemsView)
    {
        _itemsView = itemsView;
        _itemsView.TabFocusNavigation = KeyboardNavigationMode.Once;
        _itemsView.GettingFocus += OnGettingFocus;
    }

    void OnGettingFocus(UIElement sender, GettingFocusEventArgs args)
    {

        if (_redirectingFocus || args.InputDevice != FocusInputDeviceKind.Keyboard)
            return;

        if (!_itemsView.IsTabStop || _itemsView.ItemsRepeaterControl is not ItemsRepeater repeater)
        {
            args.Cancel = true;
            return;
        }

        // Guard against calling into the repeater before it has a Layout / realized containers.
        if (repeater.Layout is null || !_itemsView.IsLoaded)
        {
            args.Cancel = true;
            return;
        }

        if (args.OldFocusedElement is DependencyObject oldElement && IsDescendantOf(oldElement, _itemsView))
            return;

        var targetIndex = FindSelectedIndex(repeater, _itemsView.SelectedItem);

        if (targetIndex < 0)
            targetIndex = FindFirstItemIndex(repeater);

        if (targetIndex < 0)
            return;

        UIElement? container;

        try
        {
            container = repeater.GetOrCreateElement(targetIndex) as ItemContainer;
        }
        catch (Exception)
        {
            // Repeater wasn't actually ready despite passing the checks above — bail safely.
            return;
        }

        if (container is null || ReferenceEquals(args.NewFocusedElement, container))
            return;

        _itemsView.StartBringItemIntoView(targetIndex, new BringIntoViewOptions
        {
            AnimationDesired = false,
            VerticalAlignmentRatio = 0,
            HorizontalAlignmentRatio = 0,
        });

        _redirectingFocus = true;

        try
        {
            args.TrySetNewFocusedElement(container);
        }

        finally
        {
            _redirectingFocus = false;
        }

    }


    static int FindSelectedIndex(ItemsRepeater repeater, object? selectedItem)
    {
        if (selectedItem is null)
        {
            return -1;
        }

        var itemsSourceView = repeater.ItemsSourceView;
        for (var index = 0; index < itemsSourceView.Count; index++)
        {
            var candidate = itemsSourceView.GetAt(index);
            var actualItem = candidate is ItemTemplateContext2 itc ? itc.Item : candidate;
            if (Equals(actualItem, selectedItem))
            {
                return index;
            }
        }

        return -1;
    }

    static int FindFirstItemIndex(ItemsRepeater repeater)
    {
        for (var index = 0; index < repeater.ItemsSourceView.Count; index++)
        {
            if (repeater.ItemsSourceView.GetAt(index) is not ItemTemplateContext2 { IsHeader: true } and
                not ItemTemplateContext2 { IsFooter: true })
            {
                return index;
            }
        }

        return -1;
    }

    static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        for (var current = element; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}