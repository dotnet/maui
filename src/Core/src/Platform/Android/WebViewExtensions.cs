using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this AWebView nativeWebView, IWebView webView)
		{
			nativeWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this AWebView nativeWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);
			
				nativeWebView.UpdateCanGoBackForward(webView);	
			}
		}

		public static void UpdateSettings(this AWebView nativeWebView, IWebView webView, bool javaScriptEnabled, bool domStorageEnabled)
		{
			if (nativeWebView.Settings == null)
				return;

			nativeWebView.Settings.JavaScriptEnabled = javaScriptEnabled;
			nativeWebView.Settings.DomStorageEnabled = domStorageEnabled;
		}

		public static void UpdateGoBack(this AWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack())
				nativeWebView.GoBack();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this AWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward())
				nativeWebView.GoForward();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this AWebView nativeWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			nativeWebView.Reload();
		}

		internal static void UpdateCanGoBackForward(this AWebView nativeWebView, IWebView webView)
		{
			if (webView == null || nativeWebView == null)
				return;

			webView.CanGoBack = nativeWebView.CanGoBack();
			webView.CanGoForward = nativeWebView.CanGoForward();
		}
	}
}