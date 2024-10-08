// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
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
			if (Handler is null)
			{
				return base.ShouldInterceptRequest(view, request);
			}

			var fullUrl = request?.Url?.ToString();
			var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(fullUrl);

			if (new Uri(requestUri) is Uri uri && HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

				string? contentType = null;
				Stream? contentStream = null;

				// 1. Try special InvokeDotNet path
				if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
				{
					var fullUri = new Uri(fullUrl!);
					var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
					(var contentBytes, contentType) = Handler.InvokeDotNet(invokeQueryString);
					if (contentBytes is not null)
					{
						contentStream = new MemoryStream(contentBytes);
					}
				}

				// 2. If nothing found yet, try to get static content from the asset path
				if (contentStream is null)
				{
					if (string.IsNullOrEmpty(relativePath))
					{
						relativePath = Handler.VirtualView.DefaultFile;
						contentType = "text/html";
					}
					else
					{
						if (!HybridWebViewHandler.ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
						{
							// TODO: Log this
							contentType = "text/plain";
						}
					}

					var assetPath = Path.Combine(Handler.VirtualView.HybridRoot!, relativePath!);
					contentStream = PlatformOpenAppPackageFile(assetPath);
				}

				if (contentStream is null)
				{
					// 3.a. If still nothing is found, return a 404
					var notFoundContent = "Resource not found (404)";

					var notFoundByteArray = Encoding.UTF8.GetBytes(notFoundContent);
					var notFoundContentStream = new MemoryStream(notFoundByteArray);

					return new WebResourceResponse("text/plain", "UTF-8", 404, "Not Found", GetHeaders("text/plain"), notFoundContentStream);
				}
				else
				{
					// 3.b. Otherwise, return the content

					// TODO: We don't know the content length because Android doesn't tell us. Seems to work without it!
					return new WebResourceResponse(contentType, "UTF-8", 200, "OK", GetHeaders(contentType ?? "text/plain"), contentStream);
				}
			}
			else
			{
				return base.ShouldInterceptRequest(view, request);
			}
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
