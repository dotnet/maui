using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics.Native;
using SafariServices;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class Browser
	{
		static async Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options)
		{
			switch (options.LaunchMode)
			{
				case BrowserLaunchMode.SystemPreferred:
					var nativeUrl = new NSUrl(uri.AbsoluteUri);
					var sfViewController = new SFSafariViewController(nativeUrl, false);
					var vc = Platform.GetCurrentViewController();

					if (options.PreferredToolbarColor != null)
						sfViewController.PreferredBarTintColor = options.PreferredToolbarColor.AsUIColor();

					if (options.PreferredControlColor != null)
						sfViewController.PreferredControlTintColor = options.PreferredControlColor.AsUIColor();

					if (sfViewController.PopoverPresentationController != null)
						sfViewController.PopoverPresentationController.SourceView = vc.View;

					if (options.HasFlag(BrowserLaunchFlags.PresentAsFormSheet))
						sfViewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
					else if (options.HasFlag(BrowserLaunchFlags.PresentAsPageSheet))
						sfViewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

					await vc.PresentViewControllerAsync(sfViewController, true);
					break;
				case BrowserLaunchMode.External:
					return await Launcher.PlatformOpenAsync(uri);
			}

			return true;
		}
	}
}
