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
				// 1. Framework-internal bridge requests must be handled by the framework and
				//    never exposed to app-level WebResourceRequested interception. See
				//    IsFrameworkInternalRequest: each reserved endpoint is bound to BOTH its
				//    well-known path and (for the message/invoke channels) the protocol marker
				//    header, so the header alone is never a trust boundary. Before JS -> .NET
				//    messages were routed over HTTP they were invisible to app interception, and
				//    this preserves that invariant.
				if (IsFrameworkInternalRequest(url, request))
				{
					// A framework-internal request must be handled by the framework. If it
					// cannot be resolved, fail fast with a 400 rather than forwarding it to the
					// app handler.
					return GetResponse(url, request, logger)
						?? new WebResourceResponse(null, "UTF-8", 400, "Bad Request", null, new MemoryStream());
				}

				// 2. Check if the app wants to modify or override the request. This path is
				//    intentionally left unwrapped: if a user WebResourceRequested handler throws
				//    for a legitimate app-origin request, the exception propagates exactly as it
				//    did before bridge traffic was routed over HTTP. Only the framework's own
				//    .NET dispatch (Handler.MessageReceived in GetResponse) is exception-isolated,
				//    because it runs under a JNI stack where an unhandled throw crashes the
				//    native WebView thread.
				var response = WebRequestInterceptingWebView.TryInterceptResponseStream(Handler, view, request, url, logger);
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

		// Resolves the app-origin-relative path for a request URL. Returns false when the URL is
		// not under the HybridWebView app origin; returns true otherwise, with relativePath set to
		// the resolved path (which may itself be null if the path could not be resolved). Shared by
		// IsFrameworkInternalRequest and GetResponse to keep the URI parsing in one place.
		static bool TryGetAppRelativePath(string fullUrl, out string? relativePath)
		{
			relativePath = null;

			var requestUri = WebUtils.RemovePossibleQueryString(fullUrl);
			if (new Uri(requestUri) is not Uri uri || !HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				return false;
			}

			relativePath = WebUtils.ResolveRelativePath(HybridWebViewHandler.AppOriginUri, uri);
			return true;
		}

		// Returns true when the request targets a reserved HybridWebView bridge endpoint and must
		// therefore be handled by the framework instead of being exposed to app-level
		// WebResourceRequested interception. Each endpoint is bound to its well-known path:
		//  - the bridge bootstrap script is a plain <script> load with no header, matched by path;
		//  - the message/invoke channels must ALSO carry the protocol marker header, because the
		//    header name/value are public and a same-origin script could otherwise set it on an
		//    arbitrary URL to bypass interception.
		static bool IsFrameworkInternalRequest(string fullUrl, IWebResourceRequest request)
		{
			if (!TryGetAppRelativePath(fullUrl, out var relativePath) || relativePath is null)
			{
				return false;
			}

			if (relativePath == HybridWebViewHandler.HybridWebViewDotJsPath)
			{
				return true;
			}

			if (relativePath == HybridWebViewHandler.InvokeDotNetPath ||
				relativePath == HybridWebViewHandler.SendMessagePath)
			{
				return HybridWebViewHandler.HasExpectedHeaders(request.RequestHeaders);
			}

			return false;
		}

		private WebResourceResponse? GetResponse(string fullUrl, IWebResourceRequest request, ILogger? logger)
		{
			if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
			{
				return null;
			}

			if (!TryGetAppRelativePath(fullUrl, out var relativePath))
			{
				// Not an app-origin request; let it proceed unmodified.
				return null;
			}

			logger?.LogDebug("Request for {Url} will be handled by .NET MAUI.", fullUrl);

			if (relativePath is null)
			{
				logger?.LogDebug("Request for {Url} resolved to an invalid path.", fullUrl);
				return new WebResourceResponse("text/plain", "UTF-8", 404, "Not Found", GetHeaders("text/plain"), new MemoryStream());
			}

			// 1.a. Try the special "_framework/hybridwebview.js" path
			if (relativePath == HybridWebViewHandler.HybridWebViewDotJsPath)
			{
				logger?.LogDebug("Request for {Url} will return the hybrid web view script.", fullUrl);
				var jsStream = HybridWebViewHandler.GetEmbeddedStream(HybridWebViewHandler.HybridWebViewDotJsPath);
				if (jsStream is not null)
				{
					return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), jsStream);
				}
			}

			// 1.b. Try special InvokeDotNet path
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

			// 1.c. Try the special SendMessage path (JS -> .NET messages).
			if (relativePath == HybridWebViewHandler.SendMessagePath)
			{
				logger?.LogDebug("Request for {Url} will be handled by the .NET message receiver.", fullUrl);

				if (!TryValidateBridgeRequest(request, "SendMessage", logger, out var messageBody, out var error))
				{
					return error;
				}

				// Do not wrap this in a try/catch. MessageReceived raises the app-facing message
				// handlers (e.g. RawMessageReceived); an exception thrown by app code must be allowed
				// to propagate rather than be swallowed, matching how MAUI treats event handlers such
				// as Button.Click. Developers who want to handle these exceptions can catch them in
				// their own handler.
				Handler.MessageReceived(messageBody);

				return new WebResourceResponse(null, "UTF-8", 204, "No Content", null, new MemoryStream());
			}

			// 2. If nothing found yet, try to get static content from the asset path
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
		// ShouldInterceptRequest). On success returns true with the body in `body`;
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
