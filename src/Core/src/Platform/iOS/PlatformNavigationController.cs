using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

internal class PlatformNavigationController : UINavigationController
{
	protected NavigationViewHandler Handler { get; }

	public PlatformNavigationController(NavigationViewHandler handler)
	{
		Handler = handler;
		Delegate = new NavigationDelegate(this);
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
		var window = (Handler.MauiContext?.GetPlatformWindow().GetWindow()) ?? throw new InvalidOperationException("Could not obtain Window.");
		window.BackButtonClicked();
	}
}

internal class NavigationDelegate : UINavigationControllerDelegate
{
	WeakReference<PlatformNavigationController> NavigationController { get; }

	public NavigationDelegate(PlatformNavigationController navigationController)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
	}
}