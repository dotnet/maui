
namespace Microsoft.Maui
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
	}
}