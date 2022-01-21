using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;
		bool _firstRun = true;

		protected override AWebView CreateNativeView()
		{
			return new MauiWebView(Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};
		}

		public override void SetVirtualView(IView view)
		{
			_firstRun = true;
			base.SetVirtualView(view);
			// At this time all the mappers were already called
			_firstRun = false;
			ProcessSourceWhenReady(this, VirtualView);
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}

		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebViewClient(handler._webViewClient ??= new WebViewClient());
		}

		public static void MapWebChromeClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebChromeClient(handler._webChromeClient ??= new WebChromeClient());
		}

		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.UpdateSettings(webView, true, true);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack())
				nativeWebView.GoBack();
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward())
				nativeWebView.GoForward();
		}
	
		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			// TODO: Sync Cookies

			handler.NativeView.Reload();
		}
	
		static void ProcessSourceWhenReady(WebViewHandler handler, IWebView webView)
		{
			//We want to load the source after making sure the mapper for webclients
			//and settings were called already
			if (handler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;
			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}
	}
}
