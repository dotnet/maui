#if MACCATALYST
using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

internal class WindowViewController : UIViewController
{
	WeakReference<IView?> _iTitleBarRef;
	bool _isTitleBarVisible = true;
	internal bool IsFirstLayout { get; set; }
	internal bool HasCustomTitleBar { get; set; }

   	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in device test: 'TitleBar Does Not Leak'")]
	UIView? _titleBar;

	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in device test: 'TitleBar Does Not Leak'")]
	UIView? _contentWrapperView;

	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in device test: 'TitleBar Does Not Leak'")]
	internal NSLayoutConstraint? _contentWrapperTopConstraint;

	/// <summary>
	/// Instantiate a new <see cref="WindowViewController"/> object.
	/// </summary>
	/// <param name="contentViewController">An instance of the <see cref="UIViewController"/> that is the RootViewController.</param>
	/// <param name="window">An instance of the <see cref="IWindow"/>.</param>
	/// <param name="mauiContext">An instance of the <see cref="IMauiContext"/>.</param>
	/// <remarks>
	/// Only dragging the top of the titlebar will move the window.
	/// The top of the TitleBar will also drag the window inside of elements like buttons.
	/// Gestures such as swiping and controls like swipeview will not work inside the TitleBar.
	/// The TitleBar will not be accessible when a modal is being presented.
	/// </remarks>
	public WindowViewController(UIViewController contentViewController, IWindow window, IMauiContext mauiContext)
	{
		_iTitleBarRef = new WeakReference<IView?>(null);

		// Note: Maintain the order for adding a new ViewController to a Container ViewController
		// 1. Add the Subview
		// 2. Arrange the Subview's frame
		// 3. AddChildViewController
		// 4. Call DidMoveToParentViewController
		if (View is not null && contentViewController.View is not null)
		{
			_contentWrapperView = new UIView
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			View.AddSubview(_contentWrapperView);
			_contentWrapperView.AddSubview(contentViewController.View);
			_contentWrapperTopConstraint = _contentWrapperView.TopAnchor.ConstraintEqualTo(View.TopAnchor, 0);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				_contentWrapperView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
				_contentWrapperView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
				_contentWrapperTopConstraint,
				_contentWrapperView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
			});

		}

		SetUpTitleBar(window, mauiContext);

		AddChildViewController(contentViewController);
		contentViewController.DidMoveToParentViewController(this);
	}


	public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
	{
#pragma warning disable CA1422 // Validate platform compatibility
		base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility

		if (_titleBar is not null && _iTitleBarRef.TryGetTarget(out IView? iTitleBar) && iTitleBar?.Background is null)
		{
			_titleBar.BackgroundColor = UIColor.SecondarySystemBackground;
		}
	}

	public override void ViewWillLayoutSubviews()
	{
		LayoutTitleBar();
		base.ViewWillLayoutSubviews();
	}

	public override void ViewDidLayoutSubviews()
	{
		UpdateContentWrapperContentFrame();
		base.ViewDidLayoutSubviews();
	}

	void UpdateContentWrapperContentFrame()
	{
		// At this point the _contentWrapperView bounds haven't been set
		// so we just use the windows bounds to set this value
		var frame = new CGRect(0, 0, View!.Bounds.Width, View!.Bounds.Height - (_contentWrapperTopConstraint?.Constant ?? 0));

		if (_contentWrapperView is not null && _contentWrapperView.Subviews[0].Frame != frame)
		{
			_contentWrapperView.Subviews[0].Frame = frame;
		}
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

		var newTitleBar = window.TitleBar;
		var newPlatTitleBar = newTitleBar?.ToPlatform(mauiContext);
		HasCustomTitleBar = newPlatTitleBar is not null;

		IView? iTitleBar = null;
		_iTitleBarRef?.TryGetTarget(out iTitleBar);

		if (newTitleBar != iTitleBar)
		{
			_titleBar?.RemoveFromSuperview();
			iTitleBar?.DisconnectHandlers();
			iTitleBar = null;

			if (newPlatTitleBar is not null)
			{
				iTitleBar = newTitleBar;
				View.AddSubview(newPlatTitleBar);
			}

			_titleBar = newPlatTitleBar;
			_iTitleBarRef = new WeakReference<IView?>(iTitleBar);
		}
		
		_isTitleBarVisible = (iTitleBar?.Visibility == Visibility.Visible);

		var platformTitleBar = platformWindow.WindowScene?.Titlebar;

		if (newTitleBar is not null && platformTitleBar is not null)
		{
			platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
		}

		else if (newTitleBar is null && platformTitleBar is not null)
		{
			platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Visible;
		}

		IsFirstLayout = true;
		LayoutTitleBar();
	}

	/// <summary>
	/// Measures and arranges the TitleBar and adjusts the frame for the window content to make space for the TitleBar.
	/// </summary>
	public void LayoutTitleBar()
	{
		if (_contentWrapperTopConstraint is null || View is null)
		{
			return;
		}

		var current = _contentWrapperTopConstraint.Constant;
		_iTitleBarRef.TryGetTarget(out var iTitleBar);

		nfloat titleBarHeight = 0;

		if (_isTitleBarVisible && iTitleBar is not null)
		{
			var measured = iTitleBar.Measure(View.Bounds.Width, double.PositiveInfinity);
			iTitleBar.Arrange(new Graphics.Rect(0, 0, View.Bounds.Width, measured.Height));
			titleBarHeight = (nfloat)measured.Height;
		}

		_contentWrapperTopConstraint.Constant = titleBarHeight;

		UpdateContentWrapperContentFrame();
	}

	public void SetTitleBarVisibility(bool isVisible)
	{
		if (_contentWrapperTopConstraint is null || View is null)
			return;

		_isTitleBarVisible = isVisible;
		_iTitleBarRef.TryGetTarget(out var iTitleBar);
		iTitleBar?.InvalidateMeasure();
	}
}
#endif // MACCATALYST
