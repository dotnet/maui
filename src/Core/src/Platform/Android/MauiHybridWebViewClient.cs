// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web;
using Android.Graphics;
using Android.Webkit;
using Java.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
	public class MauiHybridWebViewClient : WebViewClient
	{
		private readonly WeakReference<HybridWebViewHandler?> _handler;

		public MauiHybridWebViewClient(HybridWebViewHandler handler)
		{
			_handler = new(handler);
		}

		private HybridWebViewHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

		// OnPageStarted — calls Reset() to clear stale scroll state.
		public override void OnPageStarted(AWebView? view, string? url, Bitmap? favicon)
		{
			RefreshViewWebViewScrollCapture.Reset(view);
			base.OnPageStarted(view, url, favicon);
		}

		// OnPageFinished — calls InjectObserver() to inject JS bridge when page loads.
		public override void OnPageFinished(AWebView? view, string? url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				base.OnPageFinished(view, url);
				return;
			}

			// Only inject the scroll-capture observer when the WebView is hosted inside
			// a RefreshView – avoids unnecessary JS overhead for standalone HybridWebViews.
			if (view is not null &&
				RefreshViewWebViewScrollCapture.IsAttached(view) &&
				RefreshViewWebViewScrollCapture.IsInsideMauiSwipeRefreshLayout(view))
			{
				RefreshViewWebViewScrollCapture.InjectObserver(view);
			}

			base.OnPageFinished(view, url);
		}

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			var url = request?.Url?.ToString();

			var logger = Handler?.MauiContext?.CreateLogger<MauiHybridWebViewClient>();

			logger?.LogDebug("Intercepting request for {Url}.", url);

			if (view is not null && request is not null && !string.IsNullOrEmpty(url))
			{
				// 1. Handle framework-internal bridge endpoints (the JS bridge script and the
				//    JS <-> .NET message/invoke channels) before user interception. A
				//    WebResourceRequested handler must never receive — or be able to throw on —
				//    these internal requests. Before JS -> .NET messages were routed over HTTP
				//    they were invisible to interception, and they should remain so.
				var bridgeResponse = TryGetBridgeResponse(url, request, logger);
				if (bridgeResponse is not null)
				{
					return bridgeResponse;
				}

				// 2. Check if the app wants to modify or override the request
				WebResourceResponse? response = null;
				try
				{
					response = WebRequestInterceptingWebView.TryInterceptResponseStream(Handler, view, request, url, logger);
				}
				catch (Exception ex)
				{
					// ShouldInterceptRequest runs on a native WebView thread; letting a user
					// WebResourceRequested handler's exception unwind across the JNI boundary
					// crashes the app. Log it and fall back to default handling instead.
					logger?.LogError(ex, "A WebResourceRequested handler threw while intercepting {Url}.", url);
				}

				if (response is not null)
				{
					return response;
				}

				// 3. Check if the request is for a local resource
				response = GetResponse(url, request, logger);
				if (response is not null)
				{
					return response;
				}
			}

			// 3. Otherwise, we let the request go through as is
			logger?.LogDebug("Request for {Url} was not handled.", url);

			return base.ShouldInterceptRequest(view, request);
		}

		// Handles the framework-internal HybridWebView bridge endpoints: the bridge script,
		// the JS -> .NET InvokeDotNet channel, and the JS -> .NET message channel. These are
		// resolved before user WebResourceRequested interception so app code never sees — or
		// can block/crash on — the framework's own control requests. Returns null when the
		// request is not one of these internal endpoints.
		private WebResourceResponse? TryGetBridgeResponse(string fullUrl, IWebResourceRequest request, ILogger? logger)
		{
			if (Handler is null || (Handler is IViewHandler ivh && ivh.VirtualView is null))
			{
				return null;
			}

			var requestUri = WebUtils.RemovePossibleQueryString(fullUrl);
			if (new Uri(requestUri) is not Uri uri || !HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				return null;
			}

			var relativePath = WebUtils.ResolveRelativePath(HybridWebViewHandler.AppOriginUri, uri);
			if (relativePath is null)
			{
				return null;
			}

			// The bridge script
			if (relativePath == HybridWebViewHandler.HybridWebViewDotJsPath)
			{
				logger?.LogDebug("Request for {Url} will return the hybrid web view script.", fullUrl);
				var jsStream = HybridWebViewHandler.GetEmbeddedStream(HybridWebViewHandler.HybridWebViewDotJsPath);
				if (jsStream is not null)
				{
					return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), jsStream);
				}

				return null;
			}

			// The JS -> .NET InvokeDotNet channel
			if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
			{
				logger?.LogDebug("Request for {Url} will be handled by the .NET method invoker.", fullUrl);

				if (!TryValidateBridgeRequest(request, "InvokeDotNet", logger, out var requestBody, out var error))
				{
					return error;
				}

				var contentBytesTask = Handler.InvokeDotNetAsync(stringBody: requestBody);
				var responseStream = new AsyncStream(contentBytesTask, logger);
				return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), responseStream);
			}

			// The JS -> .NET message channel
			if (relativePath == HybridWebViewHandler.SendMessagePath)
			{
				logger?.LogDebug("Request for {Url} will be handled by the .NET message receiver.", fullUrl);

				if (!TryValidateBridgeRequest(request, "SendMessage", logger, out var messageBody, out var error))
				{
					return error;
				}

				try
				{
					Handler.MessageReceived(messageBody);
				}
				catch (Exception ex)
				{
					// ShouldInterceptRequest runs on a native WebView thread; letting an exception
					// unwind across the JNI boundary crashes the app (and breaks in the debugger).
					// Log it and return an error response instead.
					logger?.LogError(ex, "SendMessage handler threw while processing a JS -> .NET message.");
					return new WebResourceResponse(null, "UTF-8", 500, "Internal Server Error", null, new MemoryStream());
				}

				return new WebResourceResponse(null, "UTF-8", 204, "No Content", null, new MemoryStream());
			}

			return null;
		}

		private WebResourceResponse? GetResponse(string fullUrl, IWebResourceRequest request, ILogger? logger)
		{
			if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
			{
				return null;
			}

			var requestUri = WebUtils.RemovePossibleQueryString(fullUrl);
			if (new Uri(requestUri) is not Uri uri || !HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				return null;
			}

			logger?.LogDebug("Request for {Url} will be handled by .NET MAUI.", fullUrl);

			var relativePath = WebUtils.ResolveRelativePath(HybridWebViewHandler.AppOriginUri, uri);
			if (relativePath is null)
			{
				logger?.LogDebug("Request for {Url} resolved to an invalid path.", fullUrl);
				return new WebResourceResponse("text/plain", "UTF-8", 404, "Not Found", GetHeaders("text/plain"), new MemoryStream());
			}

			// The framework-internal bridge endpoints (bridge script, InvokeDotNet and
			// SendMessage channels) are handled earlier in ShouldInterceptRequest via
			// TryGetBridgeResponse, so by this point only static app content remains.

			// Try to get static content from the asset path
			string? contentType;
			if (string.IsNullOrEmpty(relativePath))
			{
				relativePath = Handler.VirtualView.DefaultFile;
				contentType = "text/html";
			}
			else
			{
				if (!HybridWebViewHandler.ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
				{
					contentType = "text/plain";
					logger?.LogWarning("Could not determine content type for '{relativePath}'", relativePath);
				}
			}

			var assetPath = FileSystemUtils.Combine(Handler.VirtualView.HybridRoot!, relativePath!);
			var contentStream = assetPath is not null ? PlatformOpenAppPackageFile(assetPath) : null;

			if (contentStream is not null)
			{
				// 3.a. If something was found, return the content
				logger?.LogDebug("Request for {Url} will return an app package file.", fullUrl);

				// TODO: We don't know the content length because Android doesn't tell us. Seems to work without it!

				return new WebResourceResponse(contentType, "UTF-8", 200, "OK", GetHeaders(contentType ?? "text/plain"), contentStream);
			}

			// 3.b. Otherwise, return a 404
			logger?.LogDebug("Request for {Url} could not be fulfilled.", fullUrl);
			return new WebResourceResponse(null, "UTF-8", 404, "Not Found", null, null);
		}

		// Validates a POST-only bridge request that carries its body in the
		// X-Maui-Request-Body header (Android does not expose the POST body to
		// ShouldInterceptRequest). The header value is URL-encoded by the JS transport
		// and decoded here. On success returns true with the decoded body in `body`;
		// on failure returns false with the appropriate error response in `errorResponse`.
		private static bool TryValidateBridgeRequest(IWebResourceRequest request, string endpointName, ILogger? logger, [NotNullWhen(true)] out string? body, [NotNullWhen(false)] out WebResourceResponse? errorResponse)
		{
			body = null;

			if (!HybridWebViewHandler.HasExpectedHeaders(request.RequestHeaders))
			{
				logger?.LogError("{Endpoint} endpoint missing or invalid request header", endpointName);
				errorResponse = new WebResourceResponse(null, "UTF-8", 400, "Bad Request", null, null);
				return false;
			}

			if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
			{
				logger?.LogError("{Endpoint} endpoint only accepts POST requests. Received: {Method}", endpointName, request.Method);
				errorResponse = new WebResourceResponse(null, "UTF-8", 405, "Method Not Allowed", null, null);
				return false;
			}

			request.RequestHeaders?.TryGetValue(HybridWebViewHandler.InvokeDotNetBodyHeaderName, out body);
			if (string.IsNullOrEmpty(body))
			{
				logger?.LogError("{Endpoint} request missing X-Maui-Request-Body header", endpointName);
				errorResponse = new WebResourceResponse(null, "UTF-8", 400, "Bad Request", null, null);
				return false;
			}

			// The JS transport URL-encodes the header value so it survives the HTTP header byte-set
			// restriction (headers cannot carry CR/LF/NUL or non-Latin-1 characters). Decode it here
			// once for both the InvokeDotNet and SendMessage endpoints.
			body = Uri.UnescapeDataString(body);

			errorResponse = null;
			return true;
		}

		private Stream? PlatformOpenAppPackageFile(string filename)
		{
			if (Handler is null)
			{
				return null;
			}

			filename = FileSystemUtils.NormalizePath(filename);

			try
			{
				return Handler.Context.Assets?.Open(filename);
			}
			catch (Java.IO.FileNotFoundException)
			{
				return null;
			}
		}

		private protected static IDictionary<string, string> GetHeaders(string contentType) =>
			new Dictionary<string, string> {
				{ "Content-Type", contentType },
			};

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disconnect();
			}

			base.Dispose(disposing);
		}

		internal void Disconnect()
		{
			_handler.SetTarget(null);
		}
	}
}
