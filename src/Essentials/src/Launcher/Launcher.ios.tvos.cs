using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri) =>
			Task.FromResult(UIApplication.SharedApplication.CanOpenUrl(WebUtils.GetNativeUrl(uri)));

		Task<bool> PlatformOpenAsync(Uri uri) =>
			PlatformOpenAsync(WebUtils.GetNativeUrl(uri));

		Task<bool> PlatformOpenAsync(NSUrl nativeUrl) =>
			UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions());

		Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var nativeUrl = WebUtils.GetNativeUrl(uri);

			if (UIApplication.SharedApplication.CanOpenUrl(nativeUrl))
				return PlatformOpenAsync(nativeUrl);

			return Task.FromResult(false);
		}

#if __IOS__
		UIDocumentInteractionController documentController;

		Task<bool> PlatformOpenAsync(OpenFileRequest request)
		{
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			documentController = new UIDocumentInteractionController()
			{
				Name = request.File.FileName,
				Url = NSUrl.FromFilename(request.File.FullPath),
				Uti = request.File.ContentType
			};

			var view = Platform.GetCurrentUIViewController().View;

			CGRect rect;

			if (request.PresentationSourceBounds != Rect.Zero)
			{
				rect = request.PresentationSourceBounds.AsCGRect();
			}
			else
			{
				rect = DeviceInfo.Idiom == DeviceIdiom.Tablet
					? new CGRect(new CGPoint(view.Bounds.Width / 2, view.Bounds.Height), CGRect.Empty.Size)
					: view.Bounds;
			}

			documentController.PresentOpenInMenu(rect, view, true);
#pragma warning restore CA1416
			return Task.FromResult(true);
		}

#else
		Task PlatformOpenAsync(OpenFileRequest request) =>
			throw new FeatureNotSupportedException();
#endif
	}
}
