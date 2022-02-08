using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this WebView2 nativeWebView, IWebView webView)
		{
			nativeWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this WebView2 nativeWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				nativeWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateGoBack(this WebView2 nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack)
				nativeWebView.GoBack();

			nativeWebView.UpdateCanGoBackForward(webView);
		}
		
		public static void UpdateGoForward(this WebView2 nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward)
				nativeWebView.GoForward();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this WebView2 nativeWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			nativeWebView?.Reload();
		}
				
		internal static void UpdateCanGoBackForward(this WebView2 nativeWebView, IWebView webView)
		{
			webView.CanGoBack = nativeWebView.CanGoBack;
			webView.CanGoForward = nativeWebView.CanGoForward;
		}

		public static void Eval(this WebView2 nativeWebView, IWebView webView, string script)
		{ 
			if (nativeWebView == null)
				return;

			nativeWebView.DispatcherQueue.TryEnqueue(async () =>
			{
				await nativeWebView.ExecuteScriptAsync(script);
			});
		}
	}
}