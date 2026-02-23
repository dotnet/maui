using Microsoft.Extensions.Logging;
using WebKit;

namespace Microsoft.Maui.Platform;

internal static class WebRequestInterceptingWebView
{
	internal static bool TryInterceptResponseStream(IViewHandler? handler, WKWebView webView, IWKUrlSchemeTask urlSchemeTask, string url, ILogger? logger)
	{
		if (handler is null || handler.VirtualView is not IWebRequestInterceptingWebView interceptingWebView)
		{
			return false;
		}

		// 1. First, create the event args
		var platformArgs = new WebResourceRequestedEventArgs(webView, urlSchemeTask);

		// 2. Trigger the event for the app
		var handled = interceptingWebView.WebResourceRequested(platformArgs);

		// 3. If the app reported that it completed the request, then we do nothing more
		if (handled)
		{
			logger?.LogDebug("Request for {Url} was handled by the user.", url);

			return true;
		}

		return false;
	}
}
