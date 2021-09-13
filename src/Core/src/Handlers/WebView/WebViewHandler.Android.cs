using Android.Webkit;
using Android.Widget;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		protected override AWebView CreateNativeView()
		{
			var aWebView = new MauiWebView(Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};

			return aWebView;
		}
		protected override void ConnectHandler(AWebView nativeView)
		{
			WebViewMapper.UpdateProperty(this, VirtualView, nameof(AWebView.Settings));
			WebViewMapper.UpdateProperty(this, VirtualView, nameof(WebViewClient));
			WebViewMapper.UpdateProperty(this, VirtualView, nameof(WebChromeClient));
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();
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
			handler.NativeView.SetWebChromeClient(new WebChromeClient());
		}

		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.UpdateSettings(webView, true, true);
		}
	}
}