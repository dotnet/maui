
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

		public static void UpdateGoBack(this MauiWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.WebView.CanGoBack())
				platformWebView.WebView.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this MauiWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.WebView.CanGoForward())
				platformWebView.WebView.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this MauiWebView platformWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			platformWebView?.WebView.Reload();
		}

		internal static void UpdateCanGoBackForward(this MauiWebView platformWebView, IWebView webView)
		{
			webView.CanGoBack = platformWebView.WebView.CanGoBack();
			webView.CanGoForward = platformWebView.WebView.CanGoForward();
		}

		public static void Eval(this MauiWebView platformWebView, IWebView webView, string script)
		{
			platformWebView.WebView.Eval(script);
		}
	}
}