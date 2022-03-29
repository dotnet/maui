using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using SafariServices;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class BrowserImplementation : IBrowser
	{
		public async Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
		{
			switch (options.LaunchMode)
			{
				case BrowserLaunchMode.SystemPreferred:
					var nativeUrl = new NSUrl(uri.AbsoluteUri);
					Debug.Assert(!OperatingSystem.IsIOSVersionAtLeast(11));
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
					return await Launcher.OpenAsync(uri);
			}

			return true;
		}

		public Task OpenAsync(string uri)
		{
			return OpenAsync
						(
							new Uri(uri), 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = BrowserLaunchMode.SystemPreferred,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}

		public Task OpenAsync(string uri, BrowserLaunchMode launchMode)
		{
			return OpenAsync
						(
							new Uri(uri), 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = launchMode,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}
			
		public Task OpenAsync(string uri, BrowserLaunchOptions options)
		{
			return OpenAsync(new Uri(uri), options);
		}

		public Task OpenAsync(Uri uri)
		{
			return OpenAsync
						(
							uri,
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = BrowserLaunchMode.SystemPreferred,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}

		public Task OpenAsync(Uri uri, BrowserLaunchMode launchMode)
		{
			return OpenAsync
						(
							uri, 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = launchMode,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}
	}
}
