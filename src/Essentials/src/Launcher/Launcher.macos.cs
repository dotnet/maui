using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(Uri uri) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.UrlForApplication(WebUtils.GetNativeUrl(uri)) != null);

		public Task OpenAsync(Uri uri) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(WebUtils.GetNativeUrl(uri)));

		public Task<bool> TryOpenAsync(Uri uri)
		{
			var nativeUrl = WebUtils.GetNativeUrl(uri);
			var canOpen = NSWorkspace.SharedWorkspace.UrlForApplication(nativeUrl) != null;

			if (canOpen)
				return Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(nativeUrl));

			return Task.FromResult(canOpen);
		}

		public Task OpenAsync(OpenFileRequest request) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.OpenFile(request.File.FullPath));
	}
}
