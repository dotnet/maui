using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;
		bool _settingsSet;

		protected override AWebView CreateNativeView()
		{
			return new MauiWebView(Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			SetSettingsAndClient(handler);

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			if (handler._webViewClient == null)
			{
				handler._webViewClient = new WebViewClient();
				handler.NativeView.SetWebViewClient(handler._webViewClient);
			}
		}

		public static void MapWebChromeClient(WebViewHandler handler, IWebView webView)
		{
			if (handler._webChromeClient == null)
			{
				handler._webChromeClient = new WebChromeClient();
				handler.NativeView.SetWebChromeClient(handler._webChromeClient);
			}
		}

		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			if (!handler._settingsSet)
			{
				handler.NativeView.UpdateSettings(webView, true, true);
				handler._settingsSet = true;
			}
		}

		static void SetSettingsAndClient(WebViewHandler handler)
		{
			if (handler._webViewClient == null)
			{
				WebViewMapper.UpdateProperty(handler, handler.VirtualView, nameof(WebViewClient));
			}
			if (handler._webChromeClient == null)
			{
				WebViewMapper.UpdateProperty(handler, handler.VirtualView, nameof(WebChromeClient));
			}
			if (!handler._settingsSet)
			{
				WebViewMapper.UpdateProperty(handler, handler.VirtualView, nameof(AWebView.Settings));
				handler._settingsSet = true;
			}
		}
	}
}