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
	}
}