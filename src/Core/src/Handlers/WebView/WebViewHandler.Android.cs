using System;
using System.Collections.Generic;
using System.Net;
using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		internal const string AssetBaseUrl = "file:///android_asset/";

		bool _firstRun = true;
		readonly HashSet<string> _loadedCookies = new HashSet<string>();

		internal WebNavigationEvent _eventState;

		protected internal string? UrlCanceled { get; set; }

		protected override AWebView CreatePlatformView()
		{
			var platformView = new MauiWebView(this, Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
			};

			platformView.Settings.JavaScriptEnabled = true;
			platformView.Settings.DomStorageEnabled = true;
			platformView.Settings.SetSupportMultipleWindows(true);

			return platformView;
		}

		internal WebNavigationEvent CurrentNavigationEvent
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
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				if (platformView.WebViewClient is MauiWebViewClient webViewClient)
					webViewClient.Disconnect();

				if (platformView.WebChromeClient is MauiWebChromeClient webChromeClient)
					webChromeClient.Disconnect();
			}

			platformView.SetWebViewClient(null!);
			platformView.SetWebChromeClient(null);

			platformView.StopLoading();

			base.DisconnectHandler(platformView);
		}

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}

		public static void MapUserAgent(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.UpdateUserAgent(webView);
		}

		public static void MapWebViewClient(IWebViewHandler handler, IWebView webView)
		{
			if (handler is WebViewHandler platformHandler)
				handler.PlatformView.SetWebViewClient(new MauiWebViewClient(platformHandler));
		}

		public static void MapWebChromeClient(IWebViewHandler handler, IWebView webView)
		{
			if (handler is WebViewHandler platformHandler)
				handler.PlatformView.SetWebChromeClient(new MauiWebChromeClient(platformHandler));
		}

		public static void MapWebViewSettings(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.UpdateSettings(webView, true, true);
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

			string? url = handler.PlatformView.Url?.ToString();

			if (url == null)
				return;

			if (handler is WebViewHandler platformHandler)
				platformHandler.SyncPlatformCookies(url);
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

		protected internal bool NavigatingCanceled(string? url)
		{
			if (VirtualView == null || string.IsNullOrWhiteSpace(url))
				return true;

			if (url == AssetBaseUrl)
				return false;

			SyncPlatformCookies(url);
			bool cancel = VirtualView.Navigating(CurrentNavigationEvent, url);

			// if the user disconnects from the handler we want to exit
			if (!PlatformView.IsAlive())
				return true;

			PlatformView?.UpdateCanGoBackForward(VirtualView);

			UrlCanceled = cancel ? null : url;

			return cancel;
		}

		static void ProcessSourceWhenReady(IWebViewHandler handler, IWebView webView)
		{
			//We want to load the source after making sure the mapper for webclients
			//and settings were called already
			var platformHandler = handler as WebViewHandler;
			if (platformHandler == null || platformHandler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;
			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		internal void SyncPlatformCookiesToVirtualView(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromPlatformStore(url);

			if (retrieveCurrentWebCookies == null)
				return;

			foreach (Cookie cookie in cookies)
			{
				var platformCookie = retrieveCurrentWebCookies[cookie.Name];

				if (platformCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = platformCookie.Value;
			}

			SyncPlatformCookies(url);
		}

		void SyncPlatformCookies(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);

			if (cookies == null)
				return;

			var retrieveCurrentWebCookies = GetCookiesFromPlatformStore(url);

			if (retrieveCurrentWebCookies == null)
				return;

			var cookieManager = CookieManager.Instance;

			if (cookieManager == null)
				return;

			cookieManager.SetAcceptCookie(true);
			for (var i = 0; i < cookies.Count; i++)
			{
				var cookie = cookies[i];
				var cookieString = cookie.ToString();
				cookieManager.SetCookie(cookie.Domain, cookieString);
			}

			foreach (Cookie cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] != null)
					continue;

				var cookieString = $"{cookie.Name}=; max-age=0;expires=Sun, 31 Dec 2017 00:00:00 UTC";
				cookieManager.SetCookie(cookie.Domain, cookieString);
			}
		}

		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			if (!_loadedCookies.Add(uri.Host))
				return;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies != null)
			{
				var existingCookies = GetCookiesFromPlatformStore(url);

				if (existingCookies != null)
				{
					foreach (Cookie cookie in existingCookies)
					{
						if (cookies[cookie.Name] == null)
							myCookieJar.Add(cookie);
					}
				}
			}
		}

		CookieCollection? GetCookiesFromPlatformStore(string url)
		{
			CookieContainer existingCookies = new CookieContainer();
			var cookieManager = CookieManager.Instance;

			if (cookieManager == null)
				return null;

			var currentCookies = cookieManager.GetCookie(url);

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return null;

			if (currentCookies != null)
			{
				foreach (var cookie in currentCookies.Split(';'))
					existingCookies.SetCookies(uri, cookie);
			}

			return existingCookies.GetCookies(uri);
		}

		static Uri? CreateUriForCookies(string url)
		{
			if (url == null)
				return null;

			Uri? uri;

			if (url.Length > 2000)
				url = url.Substring(0, 2000);

			if (Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				if (string.IsNullOrWhiteSpace(uri.Host))
					return null;

				return uri;
			}

			return null;
		}
	}
}