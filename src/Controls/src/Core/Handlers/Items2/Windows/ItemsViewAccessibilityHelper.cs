using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Handlers.Items2;

sealed class ItemsViewAccessibilityHelper
{
    readonly MauiItemsView _itemsView;
    bool _redirectingFocus;

    // Tracks the pending ContainerPrepared callback so stale callbacks
    // can be removed when another focus request supersedes it.
    Action<int>? _pendingContainerPrepared;

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
            TryCancel(args);
            return;
        }

        if (repeater.Layout is null || !_itemsView.IsLoaded)
        {
            TryCancel(args);
            return;
        }

        if (args.OldFocusedElement is DependencyObject oldElement && IsDescendantOf(oldElement, _itemsView))
            return;

        // If WinUI already focused a real interactive control inside the CollectionView
        // (Header, Footer, GroupHeader, or EmptyView), don't redirect focus. Only
        // data-item ItemContainers should use the redirect logic below.
        if (args.NewFocusedElement is DependencyObject newElement &&
            newElement is not ItemContainer &&
            !ReferenceEquals(newElement, _itemsView) &&
            !ReferenceEquals(newElement, repeater) &&
            IsDescendantOf(newElement, _itemsView))
        {
            return;
        }

        var selectedIndex = FindSelectedIndex(repeater, _itemsView.SelectedItem);
        var targetIndex = selectedIndex >= 0 ? selectedIndex : FindFirstItemIndex(repeater);

        if (targetIndex < 0)
            return;

        if (!TryCancel(args))
            return;

        if (selectedIndex >= 0 && _itemsView.SelectionMode == ItemsViewSelectionMode.Single)
        {
            _itemsView.Select(targetIndex);
        }

        if (repeater.TryGetElement(targetIndex) is ItemContainer existingContainer)
        {
            QueueFocus(existingContainer);
            return;
        }

        _itemsView.StartBringItemIntoView(targetIndex, new BringIntoViewOptions
        {
            AnimationDesired = false,
            VerticalAlignmentRatio = 0,
            HorizontalAlignmentRatio = 0,
        });

        // Cancel any previous pending callback before registering a new one.
        CancelPendingContainerPrepared();

        void OnContainerPrepared(int preparedIndex)
        {
            if (preparedIndex != targetIndex)
                return;

            _itemsView.ContainerPrepared -= OnContainerPrepared;
            _pendingContainerPrepared = null;

            if (repeater.TryGetElement(targetIndex) is ItemContainer readyContainer)
            {
                QueueFocus(readyContainer);
            }
        }

        _pendingContainerPrepared = OnContainerPrepared;
        _itemsView.ContainerPrepared += OnContainerPrepared;
    }

    void CancelPendingContainerPreparedCore()
    {
        if (_pendingContainerPrepared is not null)
        {
            _itemsView.ContainerPrepared -= _pendingContainerPrepared;
            _pendingContainerPrepared = null;
        }
    }

    /// <summary>
    /// Attempts to cancel the GettingFocus event. Returns false (without throwing) if the
    /// event is not cancelable — which happens when the focus change is the result of
    /// window/page reactivation (e.g. navigating back to this page). WinUI does not expose
    /// a public way to detect this ahead of time, so we must attempt and catch.
    /// </summary>
    static bool TryCancel(GettingFocusEventArgs args)
    {
        try
        {
            args.Cancel = true;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    void QueueFocus(ItemContainer container)
    {
        _itemsView.DispatcherQueue.TryEnqueue(() => FocusContainer(container));
    }

    void FocusContainer(ItemContainer container)
    {
        if (container.IsLoaded)
        {
            _redirectingFocus = true;
            try
            {
                container.Focus(FocusState.Keyboard);
            }
            finally
            {
                _redirectingFocus = false;
            }

            return;
        }

        void OnLoaded(object s, RoutedEventArgs e)
        {
            container.Loaded -= OnLoaded;

            _redirectingFocus = true;
            try
            {
                container.Focus(FocusState.Keyboard);
            }
            finally
            {
                _redirectingFocus = false;
            }
        }

        container.Loaded += OnLoaded;
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
            var actualItem = candidate is ItemTemplateContext2 itc
                ? itc.Item
                : candidate;

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

    internal void CancelPendingContainerPrepared()
    {
        CancelPendingContainerPreparedCore();
    }

    internal void CleanUp()
    {
        CancelPendingContainerPreparedCore();
        _itemsView.GettingFocus -= OnGettingFocus;
    }
}
