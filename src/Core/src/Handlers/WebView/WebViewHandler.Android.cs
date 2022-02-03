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

		protected override AWebView CreatePlatformView()
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

			base.DisconnectHandler(nativeView);
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}

		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.SetWebViewClient(handler._webViewClient ??= new MauiWebViewClient(handler));
		}

		public static void MapWebChromeClient(WebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.SetWebChromeClient(handler._webChromeClient ??= new WebChromeClient());
		}

		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.UpdateSettings(webView, true, true);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView.UpdateReload(webView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		static void ProcessSourceWhenReady(WebViewHandler handler, IWebView webView)
		{
			//We want to load the source after making sure the mapper for webclients
			//and settings were called already
			if (handler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;
			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				handler.NativeView.EvaluateJavaScript(request);
			}
		}
	}
}