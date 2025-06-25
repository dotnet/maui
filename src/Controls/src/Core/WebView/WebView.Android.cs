#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public static void MapDisplayZoomControls(IWebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateDisplayZoomControls(handler.PlatformView, webView);
		}

		public static void MapEnableZoomControls(IWebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateEnableZoomControls(handler.PlatformView, webView);
		}

		public static void MapMixedContentMode(IWebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateMixedContentMode(handler.PlatformView, webView);
		}

		public static void MapJavaScriptEnabled(IWebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateJavaScriptEnabled(handler.PlatformView, webView);
		}

		public static void MapDisplayZoomControls(WebViewHandler handler, WebView webView) =>
			MapDisplayZoomControls((IWebViewHandler)handler, webView);

		public static void MapEnableZoomControls(WebViewHandler handler, WebView webView) =>
			MapEnableZoomControls((IWebViewHandler)handler, webView);

		public static void MapMixedContentMode(WebViewHandler handler, WebView webView) =>
			MapMixedContentMode((IWebViewHandler)handler, webView);
	}
}