#if MACCATALYST
using System;
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
    public UIViewController WindowContentViewController {get; set; }

	public WindowViewController(UIViewController windowContentViewController, IWindow window, IMauiContext mauiContext)
    {
        WindowContentViewController = windowContentViewController;

        AddChildViewController(windowContentViewController);

        TitleBar = window.TitleBar?.ToPlatform(mauiContext);

        var platformWindow = window.Handler?.PlatformView;
        if (TitleBar != null && platformWindow is UIWindow platWindow)
        {
            SetUpTitleBar(platWindow, mauiContext);
            _iTitleBar = window.TitleBar;

            View!.AddSubview(TitleBar);
            View!.AddSubview(_contentWrapperView);
            _contentWrapperView.AddSubview(windowContentViewController.View!);
        }   
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        UpdateTitleBar();
    }

    public void SetUpTitleBar(UIWindow platformWindow, IMauiContext mauiContext)
    {
        if (mauiContext is null)
        {
            return;
        }

        var platformTitleBar = platformWindow?.WindowScene?.Titlebar;

        if (TitleBar is not null && platformTitleBar is not null)
        {
            platformTitleBar.Toolbar = null;
            platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
        }
    }

    public void UpdateTitleBar()
    {
        if (_iTitleBar != null && View is not null)
        {
            var measured = _iTitleBar.Measure(View.Bounds.Width, double.PositiveInfinity);
            _iTitleBar.Arrange(new Graphics.Rect(0, 0, View.Bounds.Width, measured.Height));
        }

        var TitleBarHeight = TitleBar?.Frame.Height ?? 0;

        if (!_isTitleBarVisible)
        {
            TitleBarHeight = 0;
        }

        var newFrame = new CGRect(0, TitleBarHeight, View!.Bounds.Width, View!.Bounds.Height - TitleBarHeight);

        if (newFrame != _contentWrapperView.Frame)
        {
            // TODO see what happens if we remove the ContentWrapperView
            _contentWrapperView.Frame = newFrame;
            _contentWrapperView.Subviews[0].Frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - TitleBarHeight);
        }
    }

    public void SetTitleBarVisibility(bool isVisible) =>
			_isTitleBarVisible = isVisible;
}
#pragma warning restore MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
#endif // MACCATALYST
