using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation
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
			return Task.FromResult(true);
		}

#else
		Task PlatformOpenAsync(OpenFileRequest request) =>
			throw new FeatureNotSupportedException();
#endif
	}
}
