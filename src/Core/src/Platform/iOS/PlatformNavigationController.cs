using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Platform;

internal class PlatformNavigationController : UINavigationController
{
	bool _disposed;

	internal WeakReference<NavigationViewHandler> Handler { get; }
	
	WeakReference<UIToolbar> SecondaryToolbar { get; set; } = new WeakReference<UIToolbar>(new SecondaryToolbar());

	public PlatformNavigationController(NavigationViewHandler handler)
	{
		Handler = new WeakReference<NavigationViewHandler>(handler);
		Delegate = new NavigationDelegate(this, handler);
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
		if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}
		var window = (handler.MauiContext?.GetPlatformWindow().GetWindow()) ?? throw new InvalidOperationException("Could not obtain Window.");
		window.BackButtonClicked();
	}

	public override void PushViewController(UIViewController viewController, bool animated)
	{
		var containerViewController = new ParentViewController(
			Handler.TryGetTarget(out NavigationViewHandler? handler) ? handler : throw new InvalidOperationException("Could not obtain NavigationViewHandler."));

		if (TopViewController?.Title != null)
		{
			containerViewController.UpdateBackButtonTitle(TopViewController.Title);
		}

		containerViewController.View!.AddSubview(viewController.View!);
		containerViewController.AddChildViewController(viewController);
		viewController.DidMoveToParentViewController(containerViewController);

		base.PushViewController(containerViewController, animated);
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		// TODO: update translucent property?
		
		SecondaryToolbar.SetTarget(new SecondaryToolbar { Frame = new RectangleF(0, 0, 320, 44) });
		if (SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{
			View?.Add(secondaryToolbar);
			secondaryToolbar.Hidden = true;
		}
		
	}

	public override void ViewWillLayoutSubviews()
	{
		base.ViewWillLayoutSubviews();
		if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}

		if (SecondaryToolbar.TryGetTarget(out var secondaryToolbar))
		{
			UpdateToolBarVisible();

			var navBarFrameBottom = Math.Min(NavigationBar.Frame.Bottom, 140);
			var toolbar = (handler.NavigationManager?.ToolbarElement?.Toolbar) ?? throw new InvalidOperationException("Could not obtain Toolbar.");
			var hasNavigationBar = toolbar.IsVisible;

			// Use 0 if the NavBar is hidden or will be hidden
			var toolbarY = NavigationBarHidden || NavigationBar.Translucent || !hasNavigationBar ? 0 : navBarFrameBottom;
			secondaryToolbar.Frame = new RectangleF(0, (nfloat)toolbarY, View!.Frame.Width, secondaryToolbar.Frame.Height);

			handler.VirtualView.Arrange(View.Bounds.ToRectangle());
		}
	}

	void UpdateToolBarVisible()
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

		// 	if (Current?.Handler != null)
		// 		Current.ToPlatform().InvalidateMeasure(Current);

		// 	if (VisibleViewController is ParentViewController pvc)
		// 		pvc.UpdateFrames();
		}

		TopViewController?.NavigationItem?.TitleView?.SizeToFit();
		TopViewController?.NavigationItem?.TitleView?.LayoutSubviews();
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

internal class ParentViewController : UIViewController
{
	WeakReference<NavigationViewHandler> Handler { get; }

	public ParentViewController(NavigationViewHandler handler)
	{
		Handler = new WeakReference<NavigationViewHandler>(handler);
	}

	///////////////////// TODO: See UpdateFrames() in NavigationRenderer

	public override void ViewWillAppear(bool animated)
	{
		if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
		{
			throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
		}
		// TODO: not sure if we need this now that IsVisible is mapped in ToolbarHandler.iOS?
		// var toolbar = (handler.NavigationManager?.ToolbarElement?.Toolbar) ?? throw new InvalidOperationException("Could not obtain Toolbar.");
		// NavigationController?.UpdateNavigationBarVisibility(toolbar.IsVisible, animated);

		var isTranslucent = NavigationController?.NavigationBar.Translucent ?? false;
		EdgesForExtendedLayout = isTranslucent ? UIRectEdge.All : UIRectEdge.None;

		// var toolbarElement = handler.NavigationManager?.ToolbarElement;
		// var toolbarHandler = toolbarElement?.Toolbar?.Handler as ToolbarHandler;
		// toolbarHandler?._mapper.UpdateProperties(toolbarHandler, toolbarHandler.VirtualView);
		//ToolbarHandler.Mapper.UpdateProperties(toolbarHandler!, toolbarHandler!.VirtualView);

		base.ViewWillAppear(animated);
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

		handler.VirtualView.NavigationFinished(handler.NavigationStack);
	}

	//public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
	//{
	//	if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
	//	{
	//		throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
	//	}

	//	var toolbar = (handler.NavigationManager?.ToolbarElement?.Toolbar) ?? throw new InvalidOperationException("Could not obtain Toolbar.");

	//	//if (!NavigationController.TryGetTarget(out var navController))
	//	//{
	//	//	throw new InvalidOperationException("Could not obtain NavigationController.");
	//	//}
	//	viewController.NavigationController?.UpdateNavigationBarVisibility(toolbar.IsVisible, animated);

	//	var isTranslucent = navigationController.NavigationBar.Translucent;
	//	viewController.EdgesForExtendedLayout = isTranslucent ? UIRectEdge.All : UIRectEdge.None;
	//}
}