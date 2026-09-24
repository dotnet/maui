using System;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Handlers.Items2;

sealed class ItemsViewAccessibilityHelper
{
    readonly MauiItemsView _itemsView;
    bool _redirectingFocus;
    bool _attached;

    // Tracks the pending ContainerPrepared callback so stale callbacks
    // can be removed when another focus request supersedes it.
    Action<int>? _pendingContainerPrepared;

    // Index of the item that most recently held keyboard focus inside the repeater.
    // Used to restore focus to the exact item the user left, not just the selected item.
    int _lastFocusedIndex = -1;

    // Cancelled and replaced every time a new focus request is queued, and cancelled on
    // cleanup, so a dispatcher callback (or a Loaded handler it registers) can tell it has
    // been superseded/cancelled and avoid focusing a stale container.
    CancellationTokenSource? _focusCts;

    public ItemsViewAccessibilityHelper(MauiItemsView itemsView)
    {
        _itemsView = itemsView;
        _itemsView.TabFocusNavigation = KeyboardNavigationMode.Once;
        Attach();
    }

    /// <summary>
    /// (Re)subscribes to focus events. Safe to call more than once — already-attached
    /// is a no-op. Must be called again after <see cref="CleanUp"/> if the owning platform
    /// view is reconnected to a handler (e.g. a Shell tab switch or visual-tree removal).
    /// </summary>
    internal void Attach()
    {
        if (_attached)
        {
            return;
        }

        _attached = true;
        _itemsView.GettingFocus += OnGettingFocus;
        _itemsView.GotFocus += OnItemsViewGotFocus;
    }

    void OnGettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        if (_redirectingFocus || args.InputDevice != FocusInputDeviceKind.Keyboard)
        {
            return;
        }

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

        if (args.OldFocusedElement is DependencyObject oldElement)
        {
            if (IsDescendantOf(oldElement, _itemsView))
                return;

            // A previously focused element that's no longer part of the live visual tree
            // belonged to a page we've navigated away from and back to — its focus history
            // is stale, so fall back to the selected/first item instead of restoring it.
            if (oldElement is FrameworkElement { IsLoaded: false })
                _lastFocusedIndex = -1;
        }

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
        var itemCount = repeater.ItemsSourceView?.Count ?? 0;

        // Prefer the item that last held keyboard focus (covers SelectionMode.None and
        // arrow-key navigation that moves focus without changing selection). Fall back to
        // the selected item, then the first item, when there is no valid focus history.
        var targetIndex = _lastFocusedIndex >= 0 && _lastFocusedIndex < itemCount
            ? _lastFocusedIndex
            : selectedIndex >= 0 ? selectedIndex : FindFirstItemIndex(repeater);

        if (targetIndex < 0)
        {
            return;
        }

        if (!TryCancel(args))
        {
            return;
        }

        // Only re-assert selection when restoring focus to the selected item itself —
        // restoring focus to the last-focused item elsewhere must not move the selection.
        if (targetIndex == selectedIndex && _itemsView.SelectionMode == ItemsViewSelectionMode.Single)
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
            {
                return;
            }

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

    // Records the item currently holding keyboard focus so it can be restored later,
    // independently of selection (e.g. SelectionMode.None or arrow-key focus moves).
    void OnItemsViewGotFocus(object sender, RoutedEventArgs e)
    {
        if (_itemsView.ItemsRepeaterControl is not ItemsRepeater repeater)
        {
            return;
        }

        var focused = FocusManager.GetFocusedElement(_itemsView.XamlRoot) as DependencyObject;

        for (var current = focused; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is ItemContainer container)
            {
                var index = repeater.GetElementIndex(container);
                if (index >= 0)
                    _lastFocusedIndex = index;
                return;
            }

            if (ReferenceEquals(current, _itemsView))
            {
                return;
            }
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
        _focusCts?.Cancel();
        _focusCts?.Dispose();
        var cts = new CancellationTokenSource();
        _focusCts = cts;
        _itemsView.DispatcherQueue.TryEnqueue(() => FocusContainer(container, cts.Token));
    }

    // The cancellation check (both here and inside the Loaded handler below) guards against
    // focusing a stale container if this request is superseded by a newer one, or if
    // CleanUp() runs before the dispatcher callback (or Loaded event) fires.
    void FocusContainer(ItemContainer container, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (container.IsLoaded)
        {
            FocusCore(container);
            return;
        }

        void OnLoaded(object s, RoutedEventArgs e)
        {
            container.Loaded -= OnLoaded;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            FocusCore(container);
        }

        container.Loaded += OnLoaded;
    }

    void FocusCore(ItemContainer container)
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

        // Cancel any queued dispatcher/Loaded focus callback still in flight.
        _focusCts?.Cancel();
        _focusCts?.Dispose();
        _focusCts = null;

        if (_attached)
        {
            _attached = false;
            _itemsView.GettingFocus -= OnGettingFocus;
            _itemsView.GotFocus -= OnItemsViewGotFocus;
        }
    }
}
