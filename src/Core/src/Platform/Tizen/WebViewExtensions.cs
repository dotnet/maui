
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
	}
}