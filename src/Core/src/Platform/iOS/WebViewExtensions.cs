using WebKit;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this WKWebView nativeWebView, IWebView webView)
		{
			nativeWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this WKWebView nativeWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				nativeWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateGoBack(this WKWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack)
				nativeWebView.GoBack();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this WKWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward)
				nativeWebView.GoForward();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this WKWebView nativeWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			nativeWebView?.Reload();
		}

		internal static void UpdateCanGoBackForward(this WKWebView nativeWebView, IWebView webView)
		{
			webView.CanGoBack = nativeWebView.CanGoBack;
			webView.CanGoForward = nativeWebView.CanGoForward;
		}

		public static void Eval(this WKWebView nativeWebView, IWebView webView, string script)
		{
			nativeWebView.EvaluateJavaScriptAsync(script);
		}
	}
}