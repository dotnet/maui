using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui
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
				webView.Source?.Load(webViewDelegate);
		}
	}
}