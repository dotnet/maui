using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		protected override WebView2 CreateNativeView() => new MauiWebView();

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack)
				nativeWebView.GoBack();
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward)
				nativeWebView.GoForward();
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			// TODO: Sync Cookies

			handler.NativeView?.Reload();
		}
	}
}