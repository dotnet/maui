#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WItemsViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.ItemsViewSelectionChangedEventArgs;
#endif

namespace MauiApp._1.Behaviors;

public sealed class SingleSelectionKeyboardGuardBehavior : Behavior<CollectionView>
{
#if WINDOWS
    CollectionView? _collectionView;
    UIElement? _platformView;
    WItemsView? _platformItemsView;

    bool _restrictSelection;
    bool _restoringSelection;
    object? _selectedItem;
#endif

	protected override void OnAttachedTo(CollectionView bindable)
	{
		base.OnAttachedTo(bindable);

#if WINDOWS
        _collectionView = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
        AttachPlatformView(bindable);
#endif
	}

	protected override void OnDetachingFrom(CollectionView bindable)
	{
#if WINDOWS
        bindable.HandlerChanged -= OnHandlerChanged;

        DetachPlatformView();
        ClearRestriction();

        _collectionView = null;
#endif

		base.OnDetachingFrom(bindable);
	}

#if WINDOWS

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
        if (_restrictSelection)
        {
            return;
        }

        _selectedItem = _collectionView?.SelectedItem;
        _restrictSelection = true;
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

        if (ReferenceEquals(_collectionView.SelectedItem, _selectedItem))
        {
            return;
        }

        _restoringSelection = true;

        try
        {
            _collectionView.SelectedItem = _selectedItem;
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

#endif
}
