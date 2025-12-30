using System;
using System.Threading.Tasks;
using Foundation;
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
			return HandleWKWebViewResult(result);
		}

		internal static string HandleWKWebViewResult(NSObject? result)
		{
			if (result == null || result is NSNull)
			{
				return "null";
			}

			if (result is NSString nsString)
			{
				return nsString.ToString();
			}

			if (result is NSNumber nsNumber)
			{
				return nsNumber.ToString();
			}

			// For other types (NSDictionary, NSArray, etc.), use JSON serialization
			// This matches the behavior that would come from JSON.stringify() on the web side
			try
			{
				var jsonData = NSJsonSerialization.Serialize(result, NSJsonWritingOptions.PrettyPrinted, out var error);
				if (error == null && jsonData != null)
				{
					var jsonString = NSString.FromData(jsonData, NSStringEncoding.UTF8);
					return jsonString?.ToString() ?? "null";
				}
			}
			catch (Exception)
			{
				// Fall back to ToString if JSON serialization fails
				// Note: Exception is caught but not logged to avoid performance overhead in the hot path
				// If debugging is needed, consider adding conditional logging based on a flag
			}

			return result.ToString() ?? "null";
		}
	}
}
