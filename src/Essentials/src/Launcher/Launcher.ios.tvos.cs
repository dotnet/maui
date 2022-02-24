using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(string uri)
		{
			return CanOpenAsync(new Uri(uri));
		}

		public Task<bool> CanOpenAsync(Uri uri) =>
			Task.FromResult(UIApplication.SharedApplication.CanOpenUrl(WebUtils.GetNativeUrl(uri)));

		public Task OpenAsync(string uri)
		{
			return OpenAsync(new Uri(uri));
		}

		public Task OpenAsync(Uri uri) =>
			OpenAsync(WebUtils.GetNativeUrl(uri));

		public Task<bool> OpenAsync(NSUrl nativeUrl) =>
			Platform.HasOSVersion(10, 0)
				? UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions())
				: Task.FromResult(UIApplication.SharedApplication.OpenUrl(nativeUrl));

		public Task<bool> TryOpenAsync(string uri)
		{			
			return TryOpenAsync(new Uri(uri));
		}

		public Task<bool> TryOpenAsync(Uri uri)
		{
			var nativeUrl = WebUtils.GetNativeUrl(uri);

			if (UIApplication.SharedApplication.CanOpenUrl(nativeUrl))
				return OpenAsync(nativeUrl);

			return Task.FromResult(false);
		}

#if __IOS__
		static UIDocumentInteractionController documentController;

		public Task OpenAsync(OpenFileRequest request)
		{
			documentController = new UIDocumentInteractionController()
			{
				Name = request.File.FileName,
				Url = NSUrl.FromFilename(request.File.FullPath),
				Uti = request.File.ContentType
			};

			var view = Platform.GetCurrentUIViewController().View;

			CGRect rect;

			if (request.PresentationSourceBounds != Rectangle.Zero)
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
			return Task.CompletedTask;
		}

#else
		public Task OpenAsync(OpenFileRequest request) =>
			throw new FeatureNotSupportedException();
#endif
	}
}
