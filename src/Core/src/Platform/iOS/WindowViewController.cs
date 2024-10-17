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
        Console.WriteLine("ViewDidLayoutSubviews Started!");
        UpdateTitleBar();
    }

    // public override void ViewDidLoad()
    // {
    //     base.ViewDidLoad();
    //     Console.WriteLine("ViewDidLoad Started!");
    //     UpdateTitleBar();
    // }

    // public override void ViewWillLayoutSubviews()
    // {
    //     base.ViewWillLayoutSubviews();
    //     UpdateTitleBar();
    // }

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

                View!.InsertSubview(newTitleBar, 0);
            }
        }

        var platformTitleBar = platformWindow.WindowScene?.Titlebar;

        if (newTitleBar is not null && platformTitleBar is not null)
        {
            platformTitleBar.Toolbar = null;
            platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
        }

        TitleBar = newTitleBar;
        UpdateTitleBar();
    }

    public void UpdateTitleBar(IWindow window, IMauiContext mauiContext)
    {
        // TitleBar = window.TitleBar?.ToPlatform(mauiContext);
        UpdateTitleBar();
    }

    public void UpdateTitleBar()
    {
        Console.WriteLine("UpdateTitleBar Started!");
        // TODO - When the TitleBar is not showing, I'd expect the whole window to move up
        // When the titleBar is replaced, the whole window moves up to the top but not when we hide the titleBar
        if (_iTitleBar is not null && View is not null)
        {
            var heightConstraint = !_isTitleBarVisible ? 0 : double.PositiveInfinity;
            var widthConstraint = !_isTitleBarVisible ? 0 : View.Bounds.Width;

            var measured = _iTitleBar.Measure(widthConstraint, heightConstraint);
            var arranged = _iTitleBar.Arrange(new Graphics.Rect(0, 0, widthConstraint, measured.Height));

            // TODO - see what happens when the titleBar has some height when created?
        }

        // var titleBarHeight = TitleBar?.Frame.Height ?? 0;
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
        }

        
        // if (WindowContentView is not null && newFrame != WindowContentView.Frame)
        // {
        //     WindowContentView.Frame = newFrame;
        // }


        Console.WriteLine("CWV Frame: " + _contentWrapperView.Frame.Height);
        // Console.WriteLine("WCV Frame: " + WindowContentView?.Frame ?? "null");
    }

    public void SetTitleBarVisibility(bool isVisible) =>
			_isTitleBarVisible = isVisible;
}
#pragma warning restore MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
#endif // MACCATALYST
