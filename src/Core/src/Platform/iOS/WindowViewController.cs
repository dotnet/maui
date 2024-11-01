#if MACCATALYST
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

internal class WindowViewController : UIViewController
{
    WeakReference<IView?> _iTitleBarRef;
    bool _isTitleBarVisible = true;

    [UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in device test: 'TitleBar Does Not Leak'")]
    UIView? _titleBar;

    [UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in device test: 'TitleBar Does Not Leak'")]
    UIView? _contentWrapperView;

    /// <summary>
    /// Instantiate a new <see cref="WindowViewController"/> object.
    /// </summary>
    /// <param name="windowContentViewController">An instance of the <see cref="UIViewController"/> that is the RootViewController.</param>
    /// <param name="window">An instance of the <see cref="IWindow"/>.</param>
    /// <param name="mauiContext">An instance of the <see cref="IMauiContext"/>.</param>
	public WindowViewController(UIViewController windowContentViewController, IWindow window, IMauiContext mauiContext)
    {
        AddChildViewController(windowContentViewController);
        _iTitleBarRef = new WeakReference<IView?>(null);
        SetUpTitleBar(window, mauiContext);
    }

    public override void ViewWillLayoutSubviews()
    {
        base.ViewWillLayoutSubviews();
        LayoutTitleBar();
    }

    /// <summary>
    /// Sets up the TitleBar in the ViewController.
    /// </summary>
    /// <param name="window">An instance of the <see cref="IWindow"/>.</param>
    /// <param name="mauiContext">An instance of the <see cref="IMauiContext"/>.</param>
    public void SetUpTitleBar(IWindow window, IMauiContext mauiContext)
    {
        var platformWindow = window.Handler?.PlatformView as UIWindow;

        if (platformWindow is null || View is null)
        {
            return;
        }

        if (_contentWrapperView is null && ChildViewControllers.Count() > 0 
            && ChildViewControllers?[0]?.View is UIView rootView)
        {
            _contentWrapperView = new();
            View.AddSubview(_contentWrapperView);
            _contentWrapperView.AddSubview(rootView);
        }

        var newTitleBar = window.TitleBar?.ToPlatform(mauiContext);

        IView? iTitleBar = null;
        _iTitleBarRef?.TryGetTarget(out iTitleBar);

        if (newTitleBar != iTitleBar)
        {
            _titleBar?.RemoveFromSuperview();
            iTitleBar = null;

            if (newTitleBar is not null)
            {
                iTitleBar = window.TitleBar;
                View.AddSubview(newTitleBar);
            }

            _titleBar = newTitleBar;
            _iTitleBarRef = new WeakReference<IView?>(iTitleBar);
        }

        var platformTitleBar = platformWindow?.WindowScene?.Titlebar;

        if (newTitleBar is not null && platformTitleBar is not null)
        {
            platformTitleBar.Toolbar = null;
            platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
        }

        LayoutTitleBar();
        View.LayoutIfNeeded();
    }

    /// <summary>
    /// Measures and arranges the TitleBar and adjusts the frame for the window content to make space for the TitleBar.
    /// </summary>
    public void LayoutTitleBar()
    {
        _iTitleBarRef.TryGetTarget(out var iTitleBar);

        if (iTitleBar is not null && View is not null)
        {
            var measured = iTitleBar.Measure(View.Bounds.Width, double.PositiveInfinity);
            iTitleBar.Arrange(new Graphics.Rect(0, 0, View.Bounds.Width, measured.Height));
        }

        var titleBarHeight = iTitleBar?.Frame.Height ?? 0;

        if (!_isTitleBarVisible)
        {
            titleBarHeight = 0;
        }

        var newFrame = new CGRect(0, titleBarHeight, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);

        if (_contentWrapperView is not null && newFrame != _contentWrapperView.Frame && _contentWrapperView.Subviews.Count() > 0)
        {
            _contentWrapperView.Frame = newFrame;
            _contentWrapperView.Subviews[0].Frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);
        }
    }

    public void SetTitleBarVisibility(bool isVisible) =>
			_isTitleBarVisible = isVisible;
}
#endif // MACCATALYST
