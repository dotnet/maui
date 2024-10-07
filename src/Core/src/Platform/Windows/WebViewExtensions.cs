using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this WebView2 platformWebView, IWebView webView)
		{
			platformWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this WebView2 platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				platformWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateUserAgent(this WebView2 platformWebView, IWebView webView)
		{
			if (platformWebView.CoreWebView2 == null)
				return;

			if (webView.UserAgent != null)
				platformWebView.CoreWebView2.Settings.UserAgent = webView.UserAgent;
			else
				webView.UserAgent = platformWebView.CoreWebView2.Settings.UserAgent;
		}

		public static void UpdateGoBack(this WebView2 platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CoreWebView2.CanGoBack)
				platformWebView.CoreWebView2.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this WebView2 platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CoreWebView2.CanGoForward)
				platformWebView.CoreWebView2.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this WebView2 platformWebView, IWebView webView)
		{
			platformWebView?.Reload();
		}

		internal static void UpdateCanGoBackForward(this WebView2 platformWebView, IWebView webView)
		{
			webView.CanGoBack = platformWebView.CanGoBack;
			webView.CanGoForward = platformWebView.CanGoForward;
		}

		public static void Eval(this WebView2 platformWebView, IWebView webView, string script)
		{
			if (platformWebView == null)
				return;

			platformWebView.DispatcherQueue.TryEnqueue(async () =>
			{
				await platformWebView.ExecuteScriptAsync(script);
			});
		}

		public static void EvaluateJavaScript(this WebView2 webView, EvaluateJavaScriptAsyncRequest request)
		{
			request.RunAndReport(webView.ExecuteScriptAsync(request.Script));
		}

		internal static bool IsValid(this WebView2 webView)
		{
			try
			{
				return webView is not null && webView.CoreWebView2 is not null;
			}
			catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
			{
				return false;
			}
		}
	}
}