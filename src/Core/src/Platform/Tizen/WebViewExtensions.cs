using System;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this MauiWebView platformWebView, IWebView webView)
		{
			platformWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this MauiWebView platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
				webView.Source?.Load(webViewDelegate);
		}

		public static void UpdateUserAgent(this MauiWebView platformWebView, IWebView webView)
		{
			if (webView.UserAgent != null)
				platformWebView.UserAgent = webView.UserAgent;
			else
				webView.UserAgent = platformWebView.UserAgent;
		}

		public static void UpdateGoBack(this MauiWebView platformWebView, IWebView webView)
		{
			if (platformWebView.CanGoBack())
				platformWebView.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this MauiWebView platformWebView, IWebView webView)
		{
			if (platformWebView.CanGoForward())
				platformWebView.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this MauiWebView platformWebView, IWebView webView)
		{
			platformWebView.Reload();
		}

		public static void EvaluateJavaScript(this MauiWebView platformWebView, EvaluateJavaScriptAsyncRequest request)
		{
			try
			{
				platformWebView.EvaluateJavaScript(request.Script, (message) =>
				{
					request.SetResult(message);
				});
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}

		internal static void UpdateCanGoBackForward(this MauiWebView platformWebView, IWebView webView)
		{
			webView.CanGoBack = platformWebView.CanGoBack();
			webView.CanGoForward = platformWebView.CanGoForward();
		}
	}
}