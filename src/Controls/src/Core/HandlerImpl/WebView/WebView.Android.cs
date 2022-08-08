namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public static void MapDisplayZoomControls(WebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateDisplayZoomControls(handler.PlatformView, webView);
		}

		public static void MapEnableZoomControls(WebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateEnableZoomControls(handler.PlatformView, webView);
		}

		public static void MapMixedContentMode(WebViewHandler handler, WebView webView)
		{
			Platform.WebViewExtensions.UpdateMixedContentMode(handler.PlatformView, webView);
		}
	}
}