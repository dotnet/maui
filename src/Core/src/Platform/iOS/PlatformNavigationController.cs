using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Platform;

public class PlatformNavigationController : UINavigationController
{
	bool _disposed;

	public WeakReference<NavigationViewHandler> NavigationHandler { get; }
	
	public WeakReference<UIToolbar> SecondaryToolbar { get; set; } = new WeakReference<UIToolbar>(new SecondaryToolbar());

	UIImage? DefaultNavBarShadowImage { get; set; }

	bool HasNavigationBar { get; set; }

	public bool IsDisposed { get => _disposed; }

	public PlatformNavigationController(
		NavigationViewHandler handler, 
		Type? navigationBarType = null, 
		Type? toolbarType = null) : base(navigationBarType ?? typeof(UINavigationBar), toolbarType ?? typeof(UIToolbar))
	{
		NavigationHandler = new WeakReference<NavigationViewHandler>(handler);
		Delegate = new NavigationDelegate(this, handler);
		if (navigationBarType == typeof(MauiNavigationBar))
		{
			var navigationBar = new MauiNavigationBar(this);
			SetValueForKey(navigationBar, new NSString("navigationBar"));
		}
	}

	/// <summary>
	/// The UINavigationController sets itself as the delegate of the UINavigationBar automatically, 
	/// so there is no need to set it as the delegate manually here. This will be invoked when the user taps the back button. 
	/// </summary>
	[Export("navigationBar:shouldPopItem:")]
	protected virtual bool ShouldPopItem(UINavigationBar _, UINavigationItem __)
	{
		BackButtonClicked();
		return false; // the pop happens in the NavigationPage via the call above, returning true here results in an error due to the double pop
	}

	protected virtual void BackButtonClicked()
	{
		if (!NavigationHandler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}
		var window = (handler.MauiContext?.GetPlatformWindow().GetWindow()) ?? throw new InvalidOperationException("Could not obtain Window.");
		window.BackButtonClicked();
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		SecondaryToolbar.SetTarget(new SecondaryToolbar { Frame = new RectangleF(0, 0, 320, 44) });
		if (SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{
			View?.Add(secondaryToolbar);
			secondaryToolbar.Hidden = true;
		}
		
		UpdateSecondaryToolBarVisible();
	}

	public override void ViewWillLayoutSubviews()
	{
		base.ViewWillLayoutSubviews();
		if (!NavigationHandler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}

		if (SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{
			UpdateSecondaryToolBarVisible();

			var navBarFrameBottom = Math.Min(NavigationBar.Frame.Bottom, 140);
			var toolbar = (handler.NavigationManager?.ToolbarElement?.Toolbar) ?? throw new InvalidOperationException("Could not obtain Toolbar.");
			// Save the state of the current page we are calculating, this will fire before CurrentPage is updated
			HasNavigationBar = toolbar.IsVisible;

			// Use 0 if the NavBar is hidden or will be hidden
			var toolbarY = NavigationBarHidden || NavigationBar.Translucent || !HasNavigationBar ? 0 : navBarFrameBottom;
			secondaryToolbar.Frame = new RectangleF(0, (nfloat)toolbarY, View!.Frame.Width, secondaryToolbar.Frame.Height);

			handler.VirtualView.Arrange(View.Bounds.ToRectangle());
		}
	}

	public void UpdateSecondaryToolBarVisible()
	{
		if (!SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{
			return;
		}

		bool currentHidden = secondaryToolbar.Hidden;
		if (TopViewController != null && TopViewController.ToolbarItems != null && TopViewController.ToolbarItems.Length > 0)
		{
			secondaryToolbar.Hidden = false;
			secondaryToolbar.Items = TopViewController.ToolbarItems;
		}
		else
		{
			secondaryToolbar.Hidden = true;
		}

		if (currentHidden != secondaryToolbar.Hidden)
		{
			TopViewController?.InvalidateMeasure();

			if (VisibleViewController is ParentViewController pvc)
			{
				pvc.UpdateSafeArea();
			}
		}

		TopViewController?.NavigationItem?.TitleView?.SizeToFit();
		TopViewController?.NavigationItem?.TitleView?.LayoutSubviews();
	}

	// TODO: SetFlyoutLeftBarButton(UIViewController containerController, FlyoutPage FlyoutPage)?

	public void UpdateHideNavigationBarSeparator(bool shouldHideNavigationBarSeparator)
	{
		// Just setting the ShadowImage is good for iOS 11
		DefaultNavBarShadowImage ??= NavigationBar.ShadowImage;

		if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
		{
			if (shouldHideNavigationBarSeparator)
			{
				if (NavigationBar.CompactAppearance != null)
				{
					NavigationBar.CompactAppearance.ShadowColor = UIColor.Clear;
				}

				NavigationBar.StandardAppearance.ShadowColor = UIColor.Clear;

				if (NavigationBar.ScrollEdgeAppearance != null)
				{
					NavigationBar.ScrollEdgeAppearance.ShadowColor = UIColor.Clear;
				}
			}
			else
			{
				if (NavigationBar.CompactAppearance != null)
				{
					NavigationBar.CompactAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
				}
				
				NavigationBar.StandardAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
				
				if (NavigationBar.ScrollEdgeAppearance != null)
				{
					NavigationBar.ScrollEdgeAppearance.ShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
				}
			}
		}
		else
		{
			if (shouldHideNavigationBarSeparator)
			{
				NavigationBar.ShadowImage = new UIImage();
			}
			else
			{
				NavigationBar.ShadowImage = DefaultNavBarShadowImage;
			}
		}
	}

	public void UpdateHomeIndicatorAutoHidden()
	{
		SetNeedsUpdateOfHomeIndicatorAutoHidden();
	}

	public void UpdateStatusBarHidden()
	{
		SetNeedsStatusBarAppearanceUpdate();
	}

	public void ValidateNavBarExists(bool newNavigationPageHasNavBar)
	{
		if (!NavigationHandler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}

		// if the last time we did ViewDidLayoutSubviews we had another value for HasNavigationBar,
		// we will need to re-layout. This is because CurrentPage is updated async of the layout happening
		if (HasNavigationBar != newNavigationPageHasNavBar)
		{
			View!.InvalidateMeasure(handler.VirtualView);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		
		if (disposing)
		{
			Delegate.Dispose();

			if (ViewControllers != null)
			{
				foreach (var childViewController in ViewControllers)
				{
					childViewController.Dispose();
				}
			}
			
			if (SecondaryToolbar?.TryGetTarget(out var secondaryToolbar) == true)
			{
				secondaryToolbar.RemoveFromSuperview();
				secondaryToolbar.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}

public class ParentViewController : PageViewController
{
	WeakReference<NavigationViewHandler> Handler { get; }

	WeakReference<PlatformNavigationController> NavController { get; }

	bool _disposed;

	public ParentViewController(NavigationViewHandler handler, PlatformNavigationController navController, IView page, IMauiContext mauiContext) : base(page, mauiContext)
	{
		Handler = new(handler);
		NavController = new(navController);
	}

	public void UpdateSafeArea()
	{
		if (NavController.TryGetTarget(out var navigationController) &&
			ChildViewControllers.Length > 0 &&
			!navigationController.IsDisposed &&
			navigationController.SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{	
			var lastChildViewController = ChildViewControllers[^1];

			if (lastChildViewController is null)
			{
				return;
			}

			var newAdditionalSafeArea = lastChildViewController.AdditionalSafeAreaInsets;
			var offset = secondaryToolbar.Hidden ? 0 : secondaryToolbar.Frame.Height;

			if (newAdditionalSafeArea.Top != offset)
			{
				newAdditionalSafeArea.Top = offset;
				lastChildViewController.AdditionalSafeAreaInsets = newAdditionalSafeArea;
			}
		}
	}

	public override void ViewWillAppear(bool animated)
	{
		if (!NavController.TryGetTarget(out var navigationController))
		{
			throw new InvalidOperationException("Could not obtain NavigationController.");
		}

		navigationController.NavigationBar.SetupDefaultNavigationBarAppearance();

		var isTranslucent = NavigationController?.NavigationBar.Translucent ?? false;
		EdgesForExtendedLayout = isTranslucent ? UIRectEdge.All : UIRectEdge.None;

		base.ViewWillAppear(animated);
	}

	public override void ViewDidDisappear(bool animated)
	{
		base.ViewDidDisappear(animated);

		if (NavigationItem?.RightBarButtonItems == null)
		{
			return;
		}

		// force a redraw for right toolbar items by resetting TintColor to prevent
		// toolbar items being grayed out when canceling swipe to a previous page
		foreach (var item in NavigationItem?.RightBarButtonItems!)
		{
			if (item.Image != null)
			{
				continue;
			}

			var tintColor = item.TintColor;
			item.TintColor = tintColor == null ? UIColor.Clear : null;
			item.TintColor = tintColor;
		}
	}

	// TODO: do this?
	// public override void ViewWillTransitionToSize(SizeF toSize, IUIViewControllerTransitionCoordinator coordinator)
	// {
	// 	base.ViewWillTransitionToSize(toSize, coordinator);

	// 	if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
	// 		UpdateLeftBarButtonItem();
	// }

	public override void ViewDidLayoutSubviews()
	{
		base.ViewDidLayoutSubviews();
		UpdateSafeArea();
	}

	protected override void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (disposing)
		{
			if (ChildViewControllers != null)
			{
				foreach (var childViewController in ChildViewControllers)
				{
					childViewController.Dispose();
				}
			}

			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
				{
					NavigationItem.RightBarButtonItems[i].Dispose();
				}
			}

			if (ToolbarItems != null)
			{
				for (var i = 0; i < ToolbarItems.Length; i++)
				{
					ToolbarItems[i].Dispose();
				}
			}
		}

		base.Dispose(disposing);
	}
}

class SecondaryToolbar : UIToolbar
{
	readonly List<UIView> _lines = [];

	public SecondaryToolbar()
	{
		TintColor = UIColor.White;
	}

	public override UIBarButtonItem[]? Items
	{
		get { return base.Items; }
		set
		{
			base.Items = value;
			SetupLines();
		}
	}

	public override void LayoutSubviews()
	{
		base.LayoutSubviews();
		if (Items == null || Items.Length == 0)
		{
			return;
		}

		LayoutToolbarItems(Bounds.Width, Bounds.Height, 0);
	}

	void LayoutToolbarItems(nfloat toolbarWidth, nfloat toolbarHeight, nfloat padding)
	{
		var x = padding;
		var y = 0;
		var itemH = toolbarHeight;
		var itemW = toolbarWidth / Items!.Length;

		foreach (var item in Items)
		{
			var frame = new RectangleF(x, y, itemW, itemH);
			if (frame == item.CustomView!.Frame)
			{
				continue;
			}

			item.CustomView.Frame = frame;
			x += itemW + padding;
		}

		x = itemW + padding * 1.5f;
		y = (int)Bounds.GetMidY();
		foreach (var l in _lines)
		{
			l.Center = new PointF(x, y);
			x += itemW + padding;
		}
	}

	void SetupLines()
	{
		_lines.ForEach(l => l.RemoveFromSuperview());
		_lines.Clear();
		if (Items == null)
		{
			return;
		}

		for (var i = 1; i < Items.Length; i++)
		{
			var l = new UIView(new RectangleF(0, 0, 1, 24)) { BackgroundColor = new UIColor(0, 0, 0, 0.2f) };
			AddSubview(l);
			_lines.Add(l);
		}
	}
}

internal class NavigationDelegate : UINavigationControllerDelegate
{
	WeakReference<PlatformNavigationController> NavigationController { get; }
	WeakReference<NavigationViewHandler> Handler { get; }

	public NavigationDelegate(PlatformNavigationController navigationController, NavigationViewHandler handler)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
		Handler = new WeakReference<NavigationViewHandler>(handler);
	}

	public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
	{
		if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}

		if (NavigationController.TryGetTarget(out var navController) && navController.VisibleViewController is ParentViewController pvc)
		{
			pvc.UpdateSafeArea();
		}

		handler.VirtualView.NavigationFinished(handler.NavigationStack);
	}

	public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
	{
		if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}

		// Update the toolbar properties after the native navigation, since it doesn't happen automatically in the NavigationPage
		// That will clean up the toolbar settings mapped to the currently visible view controller
		var toolbarElement = handler.NavigationManager?.ToolbarElement;
		var toolbarHandler = toolbarElement?.Toolbar?.Handler as ToolbarHandler;
		toolbarHandler?._mapper.UpdateProperties(toolbarHandler, toolbarHandler.VirtualView);
	}
}

public class MauiNavigationBar : UINavigationBar
{
	public WeakReference<PlatformNavigationController> NavigationController { get; }

	public MauiNavigationBar(PlatformNavigationController navigationController)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
	}
}