﻿// Copyright (c) Microsoft Corporation.
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

			if (view is not null && request is not null)
			{
				// 1. Check if the app wants to modify or override the request
				var response = TryInterceptResponseStream(view, request, url, logger);
				if (response is not null)
				{
					return response;
				}

				// 2. Check if the request is for a local resource
				response = GetResponseStream(view, request, url, logger);
				if (response is not null)
				{
					return response;
				}
			}

			// 3. Otherwise, we let the request go through as is
			logger?.LogDebug("Request for {Url} was not handled.", url);

			return base.ShouldInterceptRequest(view, request);
		}

		private WebResourceResponse? TryInterceptResponseStream(AWebView view, IWebResourceRequest request, string? url, ILogger? logger)
		{
			if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
			{
				return null;
			}

			// 1. First, create the event args
			var platformArgs = new WebResourceRequestedEventArgs(view, request);

			// 2. Trigger the event for the app
			var handled = Handler.VirtualView.WebResourceRequested(platformArgs);

			// 3. If the app reported that it completed the request, then we do nothing more
			if (handled)
			{
				logger?.LogDebug("Request for {Url} was handled by the user.", url);

				return platformArgs.Response;
			}

			return null;
		}

		private WebResourceResponse? GetResponseStream(AWebView view, IWebResourceRequest request, string? fullUrl, ILogger? logger)
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

			var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

			// 1. Try special InvokeDotNet path
			if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
			{
				logger?.LogDebug("Request for {Url} will be handled by the .NET method invoker.", fullUrl);

				var fullUri = new Uri(fullUrl!);
				var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
				var contentBytesTask = Handler.InvokeDotNetAsync(invokeQueryString);
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

			filename = PathUtils.NormalizePath(filename);

			try
			{
				return Handler.Context.Assets?.Open(filename);
			}
			catch (Java.IO.FileNotFoundException)
			{
				return null;
			}
		}

		internal static class PathUtils
		{
			public static string NormalizePath(string filename) =>
				filename
					.Replace('\\', Path.DirectorySeparatorChar)
					.Replace('/', Path.DirectorySeparatorChar);
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
