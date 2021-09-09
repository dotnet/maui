using Android.Webkit;
using static Android.Views.ViewGroup;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView>
	{
		protected override WebView CreateNativeView()
		{
			var aWebView = new MauiWebView(Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};

			return aWebView;
		}
		protected override void ConnectHandler(WebView nativeView)
		{
			MapWebChromeClient(this, VirtualView);
			MapWebViewClient(this, VirtualView);
			MapWebViewSettings(this, VirtualView);
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(WebView nativeView)
		{
			nativeView.StopLoading();
			NativeView.WebChromeClient?.Dispose();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebViewClient(new WebViewClient());
		}

		public static void MapWebChromeClient(WebViewHandler handler, IWebView webView)
		{
			var mauiWebChromeClient = new MauiWebChromeClient();
			mauiWebChromeClient.SetContext(handler.Context);
			handler.NativeView.SetWebChromeClient(mauiWebChromeClient);
		}
		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.UpdateSettings(webView, true, true);
		}
	}
}