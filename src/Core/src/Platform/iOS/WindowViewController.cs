#if MACCATALYST
using System;
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
	/// <remarks>
	/// Only dragging the top of the titlebar will move the window.
	/// The top of the TitleBar will also drag the window inside of elements like buttons.
	/// Gestures such as swiping and controls like swipeview will not work inside the TitleBar.
	/// </remarks>
	public WindowViewController(UIViewController windowContentViewController, IWindow window, IMauiContext mauiContext)
	{
		_iTitleBarRef = new WeakReference<IView?>(null);

		// Note: Maintain the order for adding a new ViewController to a Container ViewController
		// 1. Add the Subview
		// 2. Arrange the Subview's frame
		// 3. AddChildViewController
		// 4. Call DidMoveToParentViewController

		if (View is not null && windowContentViewController.View is not null)
		{
			_contentWrapperView = new();
			View.AddSubview(_contentWrapperView);
			_contentWrapperView.AddSubview(windowContentViewController.View);
		}

		SetUpTitleBar(window, mauiContext, true);
		AddChildViewController(windowContentViewController);
		windowContentViewController.DidMoveToParentViewController(this);
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
	/// <param name="isInitalizing"></param>
	public void SetUpTitleBar(IWindow window, IMauiContext mauiContext, bool isInitalizing)
	{
		var platformWindow = window.Handler?.PlatformView as UIWindow;

		if (platformWindow is null || View is null)
		{
			return;
		}

		var newTitleBar = window.TitleBar?.ToPlatform(mauiContext);

		IView? iTitleBar = null;
		_iTitleBarRef?.TryGetTarget(out iTitleBar);

		if (newTitleBar != iTitleBar)
		{
			_titleBar?.RemoveFromSuperview();
			iTitleBar?.DisconnectHandlers();
			iTitleBar = null;

			if (newTitleBar is not null)
			{
				iTitleBar = window.TitleBar;
				SetTitleBarVisibility(iTitleBar?.Visibility == Visibility.Visible);
				View.AddSubview(newTitleBar);
			}

			_titleBar = newTitleBar;
			_iTitleBarRef = new WeakReference<IView?>(iTitleBar);
		}

		var platformTitleBar = platformWindow.WindowScene?.Titlebar;

		if (newTitleBar is not null && platformTitleBar is not null)
		{
			platformTitleBar.Toolbar = null;
			platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
		}

		// When we are initializing, calling LayoutIfNeeded will cause the layout events to not fire properly.
		// However we need this when the titlebar is added or removed or the titlebar may not be fully laid out.
		if (!isInitalizing)
		{
			View?.SetNeedsLayout();
		}
		else
		{
			LayoutTitleBar();
		}
	}

	/// <summary>
	/// Measures and arranges the TitleBar and adjusts the frame for the window content to make space for the TitleBar.
	/// </summary>
	public void LayoutTitleBar()
	{
		_iTitleBarRef.TryGetTarget(out var iTitleBar);

		if (_isTitleBarVisible && iTitleBar is not null && View is not null)
		{
			var measured = iTitleBar.Measure(View.Bounds.Width, double.PositiveInfinity);
			iTitleBar.Arrange(new Graphics.Rect(0, 0, View.Bounds.Width, measured.Height));
		}

		var titleBarHeight = iTitleBar?.Frame.Height ?? 0;

		if (!_isTitleBarVisible)
		{
			titleBarHeight = 36;
		}
		else if (titleBarHeight < 36)
		{
			titleBarHeight = 36;
		}

		var newFrame = new CGRect(0, titleBarHeight, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);

		if (_contentWrapperView is not null && newFrame != _contentWrapperView.Frame && _contentWrapperView.Subviews.Length > 0)
		{
			_contentWrapperView.Frame = newFrame;
			_contentWrapperView.Subviews[0].Frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - titleBarHeight);
		}
	}

	public void SetTitleBarVisibility(bool isVisible) =>
			_isTitleBarVisible = isVisible;
}
#endif // MACCATALYST
