// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Android.Webkit;
using Microsoft.Extensions.Logging;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform;

internal static class WebRequestInterceptingWebView
{
	internal static WebResourceResponse? TryInterceptResponseStream(IViewHandler? handler, AWebView view, IWebResourceRequest request, string url, ILogger? logger)
	{
		if (handler is null || handler.VirtualView is not IWebRequestInterceptingWebView interceptingWebView)
		{
			return null;
		}

		// 1. First, create the event args
		var platformArgs = new WebResourceRequestedEventArgs(view, request);

		// 2. Trigger the event for the app
		var handled = interceptingWebView.WebResourceRequested(platformArgs);

		// 3. If the app reported that it completed the request, then we do nothing more
		if (handled)
		{
			logger?.LogDebug("Request for {Url} was handled by the user.", url);

			return platformArgs.Response;
		}

		return null;
	}
}
