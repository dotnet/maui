using System;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal static class StaticContentCacheControl
	{
		// Historical default that disables all WebView caching of served content so that user scripts are always
		// re-executed. It is applied unless the application opts a resource in to caching via
		// BlazorWebView.StaticContentCacheControlProvider. See https://github.com/dotnet/maui/issues/8279
		internal const string Default = "no-cache, max-age=0, must-revalidate, no-store";

		// Returns the application-provided Cache-Control override for the request, or null to use the default.
		internal static string? ResolveOverride(IBlazorWebView? blazorWebView, Uri uri, string contentType)
		{
			var provider = blazorWebView?.StaticContentCacheControlProvider;
			if (provider is null)
			{
				return null;
			}

			var cacheControl = provider(new BlazorWebViewStaticContentRequest(uri, contentType));
			return string.IsNullOrEmpty(cacheControl) ? null : cacheControl;
		}
	}
}
