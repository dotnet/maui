using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.Essentials
{
	public static partial class Launcher
	{
		static Task<bool> PlatformCanOpenAsync(Uri uri) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.UrlForApplication(WebUtils.GetNativeUrl(uri)) != null);

		internal static Task PlatformOpenAsync(Uri uri) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(WebUtils.GetNativeUrl(uri)));

		static Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var nativeUrl = WebUtils.GetNativeUrl(uri);
			var canOpen = NSWorkspace.SharedWorkspace.UrlForApplication(nativeUrl) != null;

			if (canOpen)
				return Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(nativeUrl));

			return Task.FromResult(canOpen);
		}

		static Task PlatformOpenAsync(OpenFileRequest request) =>
			Task.FromResult(NSWorkspace.SharedWorkspace.OpenFile(request.File.FullPath));
	}
}
