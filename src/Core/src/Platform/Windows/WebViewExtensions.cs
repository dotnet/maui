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
				webView.Source?.Load(webViewDelegate);
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