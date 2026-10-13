//-:cnd:noEmit
#if WINDOWS
//+:cnd:noEmit

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WItemsViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.ItemsViewSelectionChangedEventArgs;


namespace MauiApp._1.Behaviors;

public sealed class SingleSelectionKeyboardGuardBehavior : Behavior<CollectionView>
{

    CollectionView? _collectionView;
    UIElement? _platformView;
    WItemsView? _platformItemsView;

    bool _restrictSelection;
    bool _restoringSelection;
    object? _selectedItem;

	protected override void OnAttachedTo(CollectionView bindable)
	{
		base.OnAttachedTo(bindable);
        _collectionView = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
        AttachPlatformView(bindable);
	}

	protected override void OnDetachingFrom(CollectionView bindable)
	{
        bindable.HandlerChanged -= OnHandlerChanged;
        DetachPlatformView();
        ClearRestriction();
        _collectionView = null;
		base.OnDetachingFrom(bindable);
	}



    void OnHandlerChanged(object? sender, EventArgs e)
    {
        AttachPlatformView(sender as CollectionView);
    }

    void AttachPlatformView(CollectionView? collectionView)
    {
        DetachPlatformView();

        var platformView = collectionView?.Handler?.PlatformView;

        _platformView = platformView as UIElement;
        _platformItemsView = platformView as WItemsView;

        if (_platformView is not null)
        {
            _platformView.PreviewKeyDown += OnPreviewKeyDown;
            _platformView.GettingFocus += OnGettingFocus;
            _platformView.LosingFocus += OnLosingFocus;
            _platformView.PointerPressed += OnPointerPressed;
        }

        if (_platformItemsView is not null)
        {
            _platformItemsView.SelectionChanged += OnPlatformSelectionChanged;
        }
    }

    void DetachPlatformView()
    {
        ClearRestriction();

        if (_platformView is not null)
        {
            _platformView.PreviewKeyDown -= OnPreviewKeyDown;
            _platformView.GettingFocus -= OnGettingFocus;
            _platformView.LosingFocus -= OnLosingFocus;
            _platformView.PointerPressed -= OnPointerPressed;
        }

        if (_platformItemsView is not null)
        {
            _platformItemsView.SelectionChanged -= OnPlatformSelectionChanged;
        }

        _platformView = null;
        _platformItemsView = null;
    }

    void OnGettingFocus(object sender, GettingFocusEventArgs e)
    {
        if (e.InputDevice == FocusInputDeviceKind.Keyboard)
        {
            RestrictSelection();
        }
    }

    void OnLosingFocus(object sender, LosingFocusEventArgs e)
    {
        if (e.NewFocusedElement is not DependencyObject focusedElement ||
            !IsDescendantOf(focusedElement, (DependencyObject)sender))
        {
            ClearRestriction();
        }
    }

    void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ClearRestriction();
    }

    void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Tab:
            case VirtualKey.Left:
            case VirtualKey.Right:
            case VirtualKey.Up:
            case VirtualKey.Down:
            case VirtualKey.Home:
            case VirtualKey.End:
            case VirtualKey.PageUp:
            case VirtualKey.PageDown:
                RestrictSelection();
                break;

            case VirtualKey.Enter:
            case VirtualKey.Space:
                ClearRestriction();
                break;

            default:
                ClearRestriction();
                break;
        }
    }

    void RestrictSelection()
    {
        if (_restrictSelection ||
            _collectionView is not { SelectionMode: SelectionMode.Single } ||
            _platformItemsView is not WItemsView platformItemsView)
        {
            return;
        }

        _selectedItem = platformItemsView.SelectedItem;
        _restrictSelection = true;

        // Limit the guard to this input turn, not subsequent programmatic selection changes.
        if (!platformItemsView.DispatcherQueue.TryEnqueue(() =>
        {
            if (ReferenceEquals(_platformItemsView, platformItemsView))
            {
                ClearRestriction();
            }
        }))
        {
            ClearRestriction();
            throw new InvalidOperationException("Unable to queue the keyboard selection guard reset.");
        }
    }

    void ClearRestriction()
    {
        _restrictSelection = false;
        _selectedItem = null;
    }

    void OnPlatformSelectionChanged(
        WItemsView sender,
        WItemsViewSelectionChangedEventArgs args)
    {
        if (!_restrictSelection ||
            _restoringSelection ||
            _collectionView is not { SelectionMode: SelectionMode.Single })
        {
            return;
        }

        if (ReferenceEquals(sender.SelectedItem, _selectedItem))
        {
            return;
        }

        _restoringSelection = true;

        try
        {
            // Restore native selection before MAUI's queued synchronization can execute commands.
            if (_selectedItem is not null &&
                sender.ItemsSource is Microsoft.UI.Xaml.Data.ICollectionView items)
            {
                for (var index = 0; index < items.Count; index++)
                {
                    if (ReferenceEquals(items[index], _selectedItem))
                    {
                        sender.Select(index);
                        return;
                    }
                }
            }

            sender.DeselectAll();
        }
        finally
        {
            _restoringSelection = false;
        }
    }

    static bool IsDescendantOf(
        DependencyObject element,
        DependencyObject ancestor)
    {
        for (var current = element;
             current is not null;
             current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
