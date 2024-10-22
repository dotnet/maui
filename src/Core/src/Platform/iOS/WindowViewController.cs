#if MACCATALYST
using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;
internal class WindowViewController : UIViewController
{
#pragma warning disable MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
	UIView _contentWrapperView = new();
    IView? _iTitleBar;
    bool _isTitleBarVisible = true;

    public UIView? TitleBar { get; set; }
    // public UIView? WindowContentView { get; set; }
    public UIViewController WindowContentViewController {get; set; }

	public WindowViewController(UIViewController windowContentViewController, IWindow window, IMauiContext mauiContext)
    {
        WindowContentViewController = windowContentViewController;

        AddChildViewController(windowContentViewController);

        View!.AddSubview(_contentWrapperView);
        _contentWrapperView.AddSubview(windowContentViewController.View!);


        // WindowContentView = windowContentViewController.View!;
        // View!.AddSubview(WindowContentView);
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        UpdateTitleBar();
    }

    public void SetUpTitleBar(IWindow window, IMauiContext mauiContext)
    {
        var platformWindow = window.Handler?.PlatformView as UIWindow;

        if (platformWindow is null)
        {
            return;
        }

        var newTitleBar = window.TitleBar?.ToPlatform(mauiContext);

        if (newTitleBar != TitleBar)
        {
            TitleBar?.RemoveFromSuperview();
            _iTitleBar = null;

            if (newTitleBar is not null)
            {
                _iTitleBar = window.TitleBar;

                View!.AddSubview(newTitleBar);
            }
        }
        // _iTitleBar = null;

        var platformTitleBar = platformWindow.WindowScene?.Titlebar;

        if (newTitleBar is not null && platformTitleBar is not null)
        {
            platformTitleBar.Toolbar = null;
            platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
        }

        TitleBar = newTitleBar;
        UpdateTitleBar();
    }

    public void UpdateTitleBar()
    {
        if (_iTitleBar is not null && View is not null)
        {
            var heightConstraint = !_isTitleBarVisible ? 0 : double.PositiveInfinity;
            var widthConstraint = !_isTitleBarVisible ? 0 : View.Bounds.Width;

            var measured = _iTitleBar.Measure(widthConstraint, heightConstraint);
            var arranged = _iTitleBar.Arrange(new Graphics.Rect(0, 0, widthConstraint, measured.Height));
        }

        var titleBarHeight = _iTitleBar?.Frame.Height ?? 0;

        if (!_isTitleBarVisible)
        {
            titleBarHeight = 0;
        }

        var newFrame = new CGRect(0, titleBarHeight, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);

        if (newFrame != _contentWrapperView.Frame && _contentWrapperView.Subviews.Count() > 0)
        {
            _contentWrapperView.Frame = newFrame;
            _contentWrapperView.Subviews[0].Frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);
            View.LayoutIfNeeded();
        }
    }

    public void SetTitleBarVisibility(bool isVisible) =>
			_isTitleBarVisible = isVisible;
}
#pragma warning restore MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
#endif // MACCATALYST
