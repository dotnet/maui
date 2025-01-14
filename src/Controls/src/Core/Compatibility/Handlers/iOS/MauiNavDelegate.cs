using System;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility;

/// <summary>
/// This class is used to adjust the navigation bar for the title bar on Mac Catalyst when a new titlebar is added or removed.
/// </summary>
internal class MauiNavDelegate : UINavigationControllerDelegate
{
	public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
	{
		if (navigationController.NavigationBar is MauiNavigationBar navBar && navBar.TitleBarNeedsRefresh)
		{
			navBar.Superview?.SetNeedsLayout();
			navBar.TitleBarNeedsRefresh = false;
		}
	}
}
