﻿using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this WKWebView platformWebView, IWebView webView)
		{
			platformWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this WKWebView platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				platformWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateUserAgent(this WKWebView platformWebView, IWebView webView)
		{
			if (webView.UserAgent != null)
				platformWebView.CustomUserAgent = webView.UserAgent;
			else
				webView.UserAgent =
					platformWebView.CustomUserAgent ??
					platformWebView.ValueForKey(new Foundation.NSString("userAgent"))?.ToString();
		}

		public static void UpdateGoBack(this WKWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoBack)
				platformWebView.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this WKWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoForward)
				platformWebView.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this WKWebView platformWebView, IWebView webView)
		{
			platformWebView?.Reload();
		}

		internal static void UpdateCanGoBackForward(this WKWebView platformWebView, IWebView webView)
		{
			webView.CanGoBack = platformWebView.CanGoBack;
			webView.CanGoForward = platformWebView.CanGoForward;
		}

		public static void Eval(this WKWebView platformWebView, IWebView webView, string script)
		{
			platformWebView.EvaluateJavaScriptAsync(script);
		}

		public static void EvaluateJavaScript(this WKWebView webView, EvaluateJavaScriptAsyncRequest request)
		{
			request.RunAndReport(EvaluateJavaScript(webView, request.Script));
		}

		static async Task<string> EvaluateJavaScript(WKWebView webView, string script)
		{
			var result = await webView.EvaluateJavaScriptAsync(script);
			return result?.ToString() ?? "null";
		}

		/// <summary>
		/// Loads a local file URL into the WebView using NSBundle resource loading.
		/// </summary>
		/// <param name="webView">The WKWebView instance to load the file into</param>
		/// <param name="url">The local file URL to load</param>
		/// <param name="logger">Optional logger for error reporting</param>
		/// <returns>True if the file was successfully loaded, false otherwise</returns>
		internal static bool LoadFile(this WKWebView webView, string url, ILogger? logger = null)
		{
			try
			{
				var file = Path.GetFileNameWithoutExtension(url);
				var ext = Path.GetExtension(url);
				var directory = Path.GetDirectoryName(url);

				// If there's a subdirectory, use the overload that accepts a subdirectory parameter else fallback to the original method if subdirectory method fails or if no subdirectory
				NSUrl? nsUrl = string.IsNullOrEmpty(directory)
					? NSBundle.MainBundle.GetUrlForResource(file, ext)
					: NSBundle.MainBundle.GetUrlForResource(file, ext, directory);

				if (nsUrl is null)
				{
					return false;
				}

				webView.LoadFileUrl(nsUrl, nsUrl);

				return true;
			}
			catch (Exception ex)
			{
				logger?.LogWarning($"Could not load {url} as local file: {ex}");
			}

			return false;
		}
	}
}