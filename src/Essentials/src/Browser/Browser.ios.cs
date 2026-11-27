#nullable enable
using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using SafariServices;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	partial class BrowserImplementation : IBrowser
	{
		public async Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
		{
			switch (options.LaunchMode)
			{
				case BrowserLaunchMode.SystemPreferred:
					await LaunchSafariViewController(uri, options);
					break;
				case BrowserLaunchMode.External:
					return await Launcher.Default.OpenAsync(uri);
			}

			return true;
		}

		private static async Task LaunchSafariViewController(Uri uri, BrowserLaunchOptions options)
		{
			var nativeUrl = new NSUrl(uri.AbsoluteUri);
			using var configuration = new SFSafariViewControllerConfiguration
			{
				EntersReaderIfAvailable = false,
			};
			var sfViewController = new SFSafariViewController(nativeUrl, configuration);
			var vc = WindowStateManager.Default.GetCurrentUIViewController(true)!;

			if (options.PreferredToolbarColor != null)
#pragma warning disable CA1422 // Toolbar tint colors are obsolete on Mac Catalyst 26+, but remain the public options' behavior
				sfViewController.PreferredBarTintColor = options.PreferredToolbarColor.AsUIColor();

			if (options.PreferredControlColor != null)
				sfViewController.PreferredControlTintColor = options.PreferredControlColor.AsUIColor();
#pragma warning restore CA1422

			if (sfViewController.PopoverPresentationController != null)
				sfViewController.PopoverPresentationController.SourceView = vc.View!;

			if (options.HasFlag(BrowserLaunchFlags.PresentAsFormSheet))
				sfViewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			else if (options.HasFlag(BrowserLaunchFlags.PresentAsPageSheet))
				sfViewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

			await vc.PresentViewControllerAsync(sfViewController, true);
		}
	}
}
