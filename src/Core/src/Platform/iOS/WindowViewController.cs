using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

// internal class WindowViewController : ContainerViewController
// {
//     CGRect _lastContainerBounds;

// 	public override void ViewDidLayoutSubviews()
// 	{
// 		// base.ViewDidLayoutSubviews();

// #if MACCATALYST
//         // _lastContainerBounds = View.Bounds;
//         var window = View?.Window;
//         var mauiWindow = window.GetWindow() as IWindow;
//         var titleBar = mauiWindow?.TitleBar;
//         // var titleBarFrame = titleBar?.PresentedContent?.Frame;
//         var windowHandler = mauiWindow?.Handler as WindowHandler;

//         if (View is not null && window is not null && windowHandler is not null && mauiWindow is not null && _lastContainerBounds != View.Bounds)
//         {
//             window.UpdateTitleBar(mauiWindow, windowHandler.MauiContext);
//         }

//         if (View is not null)
//         {
//             _lastContainerBounds = View.Bounds;
//         }
// #endif
// 	}

// }


class WindowViewController : UIViewController
{
#pragma warning disable MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
	UIView ContentWrapperView = new UIView();
	public WindowViewController(UIViewController windowContentViewController, IWindow window, IMauiContext mauiContext)
    {
        WindowContentViewController = windowContentViewController;

        AddChildViewController(windowContentViewController);

        TitleBar = window.TitleBar?.ToPlatform(mauiContext);
        if (TitleBar != null)
        {
            _iTitleBar = window.TitleBar;

            View!.AddSubview(TitleBar);
            View!.AddSubview(ContentWrapperView);
            ContentWrapperView.AddSubview(windowContentViewController.View!);   
        }   
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        if (_iTitleBar != null && View is not null)
        {
            var measured = _iTitleBar.Measure(View.Bounds.Width, double.PositiveInfinity);
            _iTitleBar.Arrange(new Graphics.Rect(0, 0, View.Bounds.Width, measured.Height));
        }

        var TitleBarHeight = TitleBar?.Frame.Height ?? 0;

        var newFrame = new CGRect(0, TitleBarHeight, View!.Bounds.Width, View!.Bounds.Height - TitleBarHeight);
        if (newFrame != ContentWrapperView.Frame)
        {
            // TODO see what happens if we remove the ContentWrapperView
            ContentWrapperView.Frame = newFrame;
            ContentWrapperView.Subviews[0].Frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - TitleBarHeight);
        }
        _frame = ContentWrapperView.Frame;
    }

    public override void LoadView()
    {
        base.LoadView();
    }
    
    CGRect _frame;
    IView? _iTitleBar;

    public UIView? TitleBar { get; set; }

    public UIViewController WindowContentViewController {get; set; }
}
#pragma warning restore MEM0002 // Reference type members in NSObject subclasses can cause memory leaks
