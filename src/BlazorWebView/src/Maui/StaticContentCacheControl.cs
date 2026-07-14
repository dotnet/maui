using System;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal static class StaticContentCacheControl
	{
		// Historical default that disables all WebView caching of served content so that user scripts are always
		// re-executed. It is applied unless the application opts a resource into caching via
		// BlazorWebView.StaticContentCacheControlProvider. See https://github.com/dotnet/maui/issues/8279
		internal const string Default = "no-cache, max-age=0, must-revalidate, no-store";

		// Returns the application-provided Cache-Control override for the request, or null to use the default.
		internal static string? ResolveOverride(IBlazorWebView? blazorWebView, string requestUri, string contentType)
		{
			var provider = blazorWebView?.StaticContentCacheControlProvider;
			if (provider is null)
			{
				return null;
			}

			// The request handlers run on background threads, so guard against a malformed URI rather than letting
			// an unexpected UriFormatException surface as a crash. If parsing fails we keep the default header.
			if (!Uri.TryCreate(requestUri, UriKind.Absolute, out var uri))
			{
				return null;
			}

			var cacheControl = provider(new BlazorWebViewStaticContentRequest(uri, contentType));

			// An empty string is deliberately treated like null (keep the default): an empty Cache-Control header
			// value is non-standard and engine-dependent, and is more likely an accidental result of string
			// manipulation than an intentional opt-in. Explicit directives are the supported way to enable caching.
			return string.IsNullOrEmpty(cacheControl) ? null : cacheControl;
		}
	}
}
