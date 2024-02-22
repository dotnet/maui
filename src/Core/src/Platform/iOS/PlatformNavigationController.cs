using System;
using Foundation;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform;

internal class PlatformNavigationController : UINavigationController
{
	internal WeakReference<NavigationViewHandler> Handler { get; }

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

		containerViewController.View!.AddSubview(viewController.View!);
		containerViewController.AddChildViewController(viewController);
		viewController.DidMoveToParentViewController(containerViewController);

		base.PushViewController(containerViewController, animated);
	}

	//public override void ViewWillLayoutSubviews()
	//{
	//	base.ViewWillLayoutSubviews();
	//	if (!Handler.TryGetTarget(out NavigationViewHandler? handler))
	//	{
	//		throw new InvalidOperationException("Could not obtain NavigationViewHandler.");
	//	}

	//	/*
	//	 UpdateToolBarVisible();

	//		var navBarFrameBottom = Math.Min(NavigationBar.Frame.Bottom, 140);
	//		var toolbar = _secondaryToolbar;

	//		//save the state of the Current page we are calculating, this will fire before Current is updated
	//		_hasNavigationBar = NavigationPage.GetHasNavigationBar(Current);
	//	 */

	//	var toolbar = handler.NavigationManager?.ToolbarElement?.Toolbar ?? throw new InvalidOperationException("Could not obtain Toolbar.");

	//	// Use 0 if the NavBar is hidden or will be hidden
	//	var toolbarY = NavigationBarHidden || NavigationBar.Translucent || !_hasNavigationBar ? 0 : navBarFrameBottom;
	//	toolbar.Frame = new RectangleF(0, toolbarY, View!.Frame.Width, toolbar.Frame.Height);
	//}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Delegate.Dispose();
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

		var toolbar = (handler.NavigationManager?.ToolbarElement?.Toolbar) ?? throw new InvalidOperationException("Could not obtain Toolbar.");
		NavigationController?.UpdateNavigationBarVisibility(toolbar.IsVisible, animated);

		base.ViewWillAppear(animated);
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