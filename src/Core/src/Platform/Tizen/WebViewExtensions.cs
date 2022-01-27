
namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this MauiWebView nativeWebView, IWebView webView)
		{
			nativeWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this MauiWebView nativeWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
				webView.Source?.Load(webViewDelegate);
		}

		public static void UpdateGoBack(this MauiWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.WebView.CanGoBack())
				nativeWebView.WebView.GoBack();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this MauiWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.WebView.CanGoForward())
				nativeWebView.WebView.GoForward();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this MauiWebView nativeWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			nativeWebView?.WebView.Reload();
		}

		internal static void UpdateCanGoBackForward(this MauiWebView nativeWebView, IWebView webView)
		{
			webView.CanGoBack = nativeWebView.WebView.CanGoBack();
			webView.CanGoForward = nativeWebView.WebView.CanGoForward();
		}

		public static void Eval(this MauiWebView nativeWebView, IWebView webView, string script)
		{
			nativeWebView.WebView.Eval(script);
		}
	}
}