using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal static class StaticContentCacheControl
	{
		// Historical default that disables all WebView caching of served content so that user scripts are always
		// re-executed. It is applied unless the application opts a resource into caching via
		// BlazorWebView.StaticContentCacheControlProvider. See https://github.com/dotnet/maui/issues/8279
		internal const string Default = "no-cache, max-age=0, must-revalidate, no-store";

		// Returns the application-provided Cache-Control override for the request, or null to use the default.
		internal static string? ResolveOverride(IBlazorWebView? blazorWebView, string requestUri, string contentType, ILogger? logger)
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

			string? cacheControl;
			try
			{
				cacheControl = provider(new BlazorWebViewStaticContentRequest(uri, contentType));
			}
			catch (Exception ex)
			{
				// The provider is arbitrary application code invoked from the native request-handling path. On Windows
				// it runs inside an async void handler, where an escaped exception would also skip deferral.Complete()
				// and hang the request. A faulty provider must not take down static asset serving, so keep the default.
				logger?.StaticContentCacheControlProviderFailed(requestUri, ex);
				return null;
			}

			// An empty string is deliberately treated like null (keep the default): an empty Cache-Control header
			// value is non-standard and engine-dependent, and is more likely an accidental result of string
			// manipulation than an intentional opt-in. Explicit directives are the supported way to enable caching.
			if (string.IsNullOrEmpty(cacheControl))
			{
				return null;
			}

			// Values containing CR/LF are also rejected: some platforms concatenate the value into a raw response
			// header block, so a stray newline would produce a malformed response or allow header injection.
			if (cacheControl.Contains('\r', StringComparison.Ordinal) || cacheControl.Contains('\n', StringComparison.Ordinal))
			{
				return null;
			}

			return cacheControl;
		}
	}
}
