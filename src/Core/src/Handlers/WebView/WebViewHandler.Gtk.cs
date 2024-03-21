using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, MauiWebView>
	{
		internal const string AssetBaseUrl = MauiWebView.AssetBaseUrl;

		bool _firstRun = true;
		internal WebNavigationEvent _eventState;

		internal WebNavigationEvent CurrentNavigationEvent
		{
			get => _eventState;
			set => _eventState = value;
		}

		protected internal string? UrlCanceled { get; set; }

		protected override MauiWebView CreatePlatformView()
		{
			return new(this);
		}

		public override void SetVirtualView(IView view)
		{
			_firstRun = true;
			base.SetVirtualView(view);
			// At this time all the mappers were already called
			_firstRun = false;
			ProcessSourceWhenReady(this, VirtualView);
		}

		protected override void ConnectHandler(MauiWebView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.UpdateSettings(VirtualView, true, true);
		}

		protected override void DisconnectHandler(MauiWebView platformView)
		{
			platformView.StopLoading();

			base.DisconnectHandler(platformView);
		}

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}

		public static void MapGoBack(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoBack() && handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Back;

			handler.PlatformView.UpdateGoBack(webView);
		}

		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoForward() && handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Forward;

			handler.PlatformView.UpdateGoForward(webView);
		}

		public static void MapReload(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Refresh;

			handler.PlatformView.UpdateReload(webView);

			string? url = handler.PlatformView.Uri;

			if (url == null)
				return;
		}

		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		public static void MapEvaluateJavaScriptAsync(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				handler.PlatformView.EvaluateJavaScript(request);
			}
		}

		public static void MapUserAgent(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.UpdateUserAgent(webView);
		}

		static void ProcessSourceWhenReady(IWebViewHandler handler, IWebView webView)
		{
			//We want to load the source after making sure the mapper for webclients
			//and settings were called already
			var platformHandler = handler as WebViewHandler;

			if (platformHandler == null || platformHandler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;
			handler.PlatformView.UpdateSource(webView, webViewDelegate);
		}

		protected internal bool NavigatingCanceled(string? url)
		{
			if (VirtualView == null || string.IsNullOrWhiteSpace(url))
				return true;

			if (url == AssetBaseUrl)
				return false;

			// SyncPlatformCookies(url);
			bool cancel = VirtualView.Navigating(CurrentNavigationEvent, url);

			// if the user disconnects from the handler we want to exit
			if (!PlatformView.IsLoaded())
				return true;

			PlatformView?.UpdateCanGoBackForward(VirtualView);

			UrlCanceled = cancel ? null : url;

			return cancel;
		}
	}
}