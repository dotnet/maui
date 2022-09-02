using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Web.Http;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		WebNavigationEvent _eventState;
		readonly HashSet<string> _loadedCookies = new HashSet<string>();

		protected override WebView2 CreatePlatformView() => new MauiWebView();

		internal WebNavigationEvent CurrentNavigationEvent
		{
			get => _eventState;
			set => _eventState = value;
		}

		protected override void ConnectHandler(WebView2 platformView)
		{
			platformView.CoreWebView2Initialized += OnCoreWebView2Initialized;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
			if (platformView.CoreWebView2 != null)
			{
				platformView.CoreWebView2.HistoryChanged -= OnHistoryChanged;
				platformView.CoreWebView2.NavigationStarting -= OnNavigationStarting;
				platformView.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
			}

			platformView.CoreWebView2Initialized -= OnCoreWebView2Initialized;

			base.DisconnectHandler(platformView);
		}

		void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
		{
			sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
			sender.CoreWebView2.NavigationStarting += OnNavigationStarting;
			sender.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
		}

		void OnHistoryChanged(CoreWebView2 sender, object args)
		{
			PlatformView?.UpdateCanGoBackForward(VirtualView);
		}

		void OnNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			if (args.IsSuccess)
				NavigationSucceeded(sender, args);
			else
				NavigationFailed(sender, args);
		}

		void OnNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
		{
			if (Uri.TryCreate(args.Uri, UriKind.Absolute, out Uri? uri) && uri != null)
			{
				bool cancel = VirtualView.Navigating(CurrentNavigationEvent, uri.AbsoluteUri);

				args.Cancel = cancel;

				// Reset in this case because this is the last event we will get
				if (cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
		}

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;

			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoBack && handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Back;

			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoForward && handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Forward;

			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler is WebViewHandler w)
				w.CurrentNavigationEvent = WebNavigationEvent.Refresh;

			handler.PlatformView?.UpdateReload(webView);
		}

		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		void NavigationSucceeded(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			var uri = sender.Source;

			if (uri != null)
				SendNavigated(uri, CurrentNavigationEvent, WebNavigationResult.Success);

			if (VirtualView is null)
				return;

			PlatformView?.UpdateCanGoBackForward(VirtualView);
		}

		void NavigationFailed(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			var uri = sender.Source;

			if (!string.IsNullOrEmpty(uri))
				SendNavigated(uri, CurrentNavigationEvent, WebNavigationResult.Failure);
		}

		void SendNavigated(string url, WebNavigationEvent evnt, WebNavigationResult result)
		{
			if (VirtualView != null)
			{
				SyncPlatformCookiesToVirtualView(url);

				VirtualView.Navigated(evnt, url, result);

				PlatformView?.UpdateCanGoBackForward(VirtualView);
			}

			CurrentNavigationEvent = WebNavigationEvent.NewPage;
		}

		void SyncPlatformCookiesToVirtualView(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromPlatformStore(url);

			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var platformCookies = filter.CookieManager.GetCookies(uri);

			foreach (Cookie cookie in cookies)
			{
				var httpCookie = platformCookies
					.FirstOrDefault(x => x.Name == cookie.Name);

				if (httpCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = httpCookie.Value;
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

			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();

			foreach (Cookie cookie in cookies)
			{
				HttpCookie httpCookie = new HttpCookie(cookie.Name, cookie.Domain, cookie.Path)
				{
					Value = cookie.Value
				};
				filter.CookieManager.SetCookie(httpCookie, false);
			}

			foreach (HttpCookie cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] != null)
					continue;

				filter.CookieManager.DeleteCookie(cookie);
			}
		}

		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = new Uri(url);

			if (!_loadedCookies.Add(uri.Host))
				return;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies != null)
			{
				var existingCookies = GetCookiesFromPlatformStore(url);
				foreach (HttpCookie cookie in existingCookies)
				{
					if (cookies[cookie.Name] == null)
						myCookieJar.SetCookies(uri, cookie.ToString());
				}
			}
		}

		HttpCookieCollection GetCookiesFromPlatformStore(string url)
		{
			var uri = CreateUriForCookies(url);
			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var platformCookies = filter.CookieManager.GetCookies(uri);

			return platformCookies;
		}

		Uri? CreateUriForCookies(string url)
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



		public static void MapEvaluateJavaScriptAsync(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				if (handler.PlatformView == null)
				{
					request.SetCanceled();
					return;
				}

				handler.PlatformView.EvaluateJavaScript(request);
			}
		}
	}
}