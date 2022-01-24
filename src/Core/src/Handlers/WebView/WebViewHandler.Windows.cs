using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		protected override WebView2 CreateNativeView() => new MauiWebView();

		protected override void ConnectHandler(WebView2 nativeView)
		{
			nativeView.NavigationCompleted += OnNavigationCompleted;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(WebView2 nativeView)
		{
			nativeView.NavigationCompleted -= OnNavigationCompleted;

			base.DisconnectHandler(nativeView);
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView?.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView?.UpdateReload(webView);
		}

		void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			if (VirtualView == null)
				return;

			sender.UpdateCanGoBackForward(VirtualView);
		}
	}
}