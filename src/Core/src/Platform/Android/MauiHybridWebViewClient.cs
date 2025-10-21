// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web;
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

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			var url = request?.Url?.ToString();

			var logger = Handler?.MauiContext?.CreateLogger<MauiHybridWebViewClient>();

			logger?.LogDebug("Intercepting request for {Url}.", url);

			if (view is not null && request is not null && !string.IsNullOrEmpty(url))
			{
				// 1. Check if the app wants to modify or override the request
				var response = WebRequestInterceptingWebView.TryInterceptResponseStream(Handler, view, request, url, logger);
				if (response is not null)
				{
					return response;
				}

				// 2. Check if the request is for a local resource
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

			var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString();

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

				// Only accept requests that have the expected headers
				if (!HybridWebViewHandler.HasExpectedHeaders(request.RequestHeaders))
				{
					logger?.LogError("InvokeDotNet endpoint missing or invalid request header");
					return new WebResourceResponse(null, "UTF-8", 400, "Bad Request", null, null);
				}

				// Only accept POST requests
				if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
				{
					logger?.LogError("InvokeDotNet endpoint only accepts POST requests. Received: {Method}", request.Method);
					return new WebResourceResponse(null, "UTF-8", 405, "Method Not Allowed", null, null);
				}

				// On Android, POST body is not available in ShouldInterceptRequest, so we use a custom header
				var requestBody = request.RequestHeaders?[HybridWebViewHandler.InvokeDotNetBodyHeaderName];
				if (string.IsNullOrEmpty(requestBody))
				{
					logger?.LogError("InvokeDotNet request missing X-Maui-Request-Body header");
					return new WebResourceResponse(null, "UTF-8", 400, "Bad Request", null, null);
				}

				var contentBytesTask = Handler.InvokeDotNetAsync(stringBody: requestBody);
				var responseStream = new AsyncStream(contentBytesTask, logger);
				return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), responseStream);
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

			var assetPath = Path.Combine(Handler.VirtualView.HybridRoot!, relativePath!);
			var contentStream = PlatformOpenAppPackageFile(assetPath);

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
