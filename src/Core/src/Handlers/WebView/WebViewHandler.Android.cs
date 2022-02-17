using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;
		bool _firstRun = true;

		internal WebNavigationEvent _eventState;

		protected internal string? UrlCanceled { get; set; }

		protected override AWebView CreatePlatformView()
		{
			return new MauiWebView(this, Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};
		}

		internal WebNavigationEvent CurrentWebNavigationEvent
		{
			get => _eventState;
			set => _eventState = value;
		}

		public override void SetVirtualView(IView view)
		{
			_firstRun = true;
			base.SetVirtualView(view);
			// At this time all the mappers were already called
			_firstRun = false;
			ProcessSourceWhenReady(this, VirtualView);
		}

		protected override void DisconnectHandler(AWebView platformView)
		{
			platformView.StopLoading();

			base.DisconnectHandler(platformView);
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
			if (handler.PlatformView.CanGoBack())
				handler.CurrentWebNavigationEvent = WebNavigationEvent.Back;
						
			handler.PlatformView.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoForward())
				handler.CurrentWebNavigationEvent = WebNavigationEvent.Forward;

			handler.PlatformView.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.CurrentWebNavigationEvent = WebNavigationEvent.Refresh;
			handler.PlatformView.UpdateReload(webView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		protected internal bool NavigatingCanceled(string? url)
		{
			if (VirtualView == null || string.IsNullOrWhiteSpace(url))
				return true;

			if (url == AssetBaseUrl)
				return false;

			// TODO: Sync Cookies
			bool cancel = VirtualView.Navigating(_eventState, url);
			PlatformView?.UpdateCanGoBackForward(VirtualView);
			UrlCanceled = cancel ? null : url;

			return cancel;
		}

		static void ProcessSourceWhenReady(WebViewHandler handler, IWebView webView)
		{
			// We want to load the source after making sure the mapper for webclients
			// and settings were called already
			if (handler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;
			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				handler.PlatformView.EvaluateJavaScript(request);
			}
		}
	}
}