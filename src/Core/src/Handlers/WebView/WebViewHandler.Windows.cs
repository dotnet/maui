using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		WebNavigationEvent _eventState;
		readonly WebView2Proxy _proxy = new();
		readonly HashSet<string> _loadedCookies = new();

		protected override WebView2 CreatePlatformView() => new MauiWebView(this);

		internal WebNavigationEvent CurrentNavigationEvent
		{
			get => _eventState;
			set => _eventState = value;
		}

		protected override void ConnectHandler(WebView2 platformView)
		{
			_proxy.Connect(this, platformView);
			base.ConnectHandler(platformView);

			if (platformView.IsLoaded)
				OnLoaded();
			else
				platformView.Loaded += OnWebViewLoaded;
		}

		void OnWebViewLoaded(object sender, RoutedEventArgs e)
		{
			OnLoaded();
		}

		void OnLoaded()
		{
			var window = MauiContext!.GetPlatformWindow();
			_proxy.Connect(window);
		}

		void Disconnect(WebView2 platformView)
		{
			platformView.Loaded -= OnWebViewLoaded;
			_proxy.Disconnect(platformView);
			if (platformView.CoreWebView2 is not null)
			{
				platformView.Close();
			}
		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
			Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		void OnNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
		{
			if (Uri.TryCreate(args.Uri, UriKind.Absolute, out Uri? uri) && uri is not null)
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

		public static void MapUserAgent(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView?.UpdateUserAgent(webView);
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

			if (uri is not null)
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

		// ProcessFailed is raised when a WebView process ends unexpectedly or becomes unresponsive.
		void ProcessFailed(CoreWebView2 sender, CoreWebView2ProcessFailedEventArgs args)
		{
			SendProcessFailed(args);
		}

		async void SendNavigated(string url, WebNavigationEvent evnt, WebNavigationResult result)
		{
			if (VirtualView is not null)
			{
				await SyncPlatformCookiesToVirtualView(url);

				VirtualView.Navigated(evnt, url, result);
				PlatformView?.UpdateCanGoBackForward(VirtualView);
			}

			CurrentNavigationEvent = WebNavigationEvent.NewPage;
		}

		void SendProcessFailed(CoreWebView2ProcessFailedEventArgs args)
		{
			VirtualView?.ProcessTerminated(new WebProcessTerminatedEventArgs(PlatformView.CoreWebView2, args));
		}

		async Task SyncPlatformCookiesToVirtualView(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar is null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri is null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = await GetCookiesFromPlatformStore(url);

			var platformCookies = await PlatformView.CoreWebView2.CookieManager.GetCookiesAsync(uri.AbsoluteUri);

			foreach (Cookie cookie in cookies)
			{
				var httpCookie = platformCookies
					.FirstOrDefault(x => x.Name == cookie.Name);

				if (httpCookie is null)
					cookie.Expired = true;
				else
					cookie.Value = httpCookie.Value;
			}

			await SyncPlatformCookies(url);
		}

		internal async Task SyncPlatformCookies(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri is null)
				return;

			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar is null)
				return;

			if (PlatformView.CoreWebView2 is null)
			{
				return;
			}

			await InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);

			if (cookies is null)
				return;

			var retrieveCurrentWebCookies = await GetCookiesFromPlatformStore(url);

			foreach (Cookie cookie in cookies)
			{
				var createdCookie = PlatformView.CoreWebView2.CookieManager.CreateCookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path);
				PlatformView.CoreWebView2.CookieManager.AddOrUpdateCookie(createdCookie);
			}

			foreach (CoreWebView2Cookie cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] is not null)
					continue;

				PlatformView.CoreWebView2.CookieManager.DeleteCookie(cookie);
			}
		}

		async Task InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar is null)
				return;

			var uri = new Uri(url);

			if (!_loadedCookies.Add(uri.Host))
				return;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies is not null)
			{
				var existingCookies = await GetCookiesFromPlatformStore(url);

				if (existingCookies.Count == 0)
					return;

				foreach (CoreWebView2Cookie cookie in existingCookies)
				{
					// TODO Ideally we use cookie.ToSystemNetCookie() here, but it's not available for some reason check back later
					if (cookies[cookie.Name] is null)
						myCookieJar.SetCookies(uri,
							new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain)
							{
								Expires = DateTimeOffset.FromUnixTimeMilliseconds((long)cookie.Expires).DateTime,
								HttpOnly = cookie.IsHttpOnly,
								Secure = cookie.IsSecure,
							}.ToString());
				}
			}
		}

		Task<IReadOnlyList<CoreWebView2Cookie>> GetCookiesFromPlatformStore(string url)
		{
			return PlatformView.CoreWebView2.CookieManager.GetCookiesAsync(url).AsTask();
		}

		static Uri? CreateUriForCookies(string url)
		{
			if (url is null)
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
				if (handler.PlatformView is null)
				{
					request.SetCanceled();
					return;
				}

				handler.PlatformView.EvaluateJavaScript(request);
			}
		}

		class WebView2Proxy
		{
			WeakReference<Window>? _window;
			WeakReference<WebViewHandler>? _handler;

			Window? Window => _window is not null && _window.TryGetTarget(out var w) ? w : null;
			WebViewHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

			public void Connect(WebViewHandler handler, WebView2 platformView)
			{
				_handler = new(handler);
				platformView.CoreWebView2Initialized += OnCoreWebView2Initialized;
			}

			public void Connect(Window window)
			{
				_window = new(window);
				window.Closed += OnWindowClosed;
			}

			public void Disconnect(WebView2 platformView)
			{
				platformView.CoreWebView2Initialized -= OnCoreWebView2Initialized;

				if (platformView.CoreWebView2 is CoreWebView2 webView2)
				{
					webView2.HistoryChanged -= OnHistoryChanged;
					webView2.NavigationStarting -= OnNavigationStarting;
					webView2.NavigationCompleted -= OnNavigationCompleted;
					webView2.ProcessFailed -= OnProcessFailed;
					webView2.Stop();
				}

				if (Window is Window window)
				{
					window.Closed -= OnWindowClosed;
				}

				_handler = null;
				_window = null;
			}

			void OnWindowClosed(object sender, WindowEventArgs args)
			{
				if (Handler is WebViewHandler handler)
				{
					handler.Disconnect(handler.PlatformView);
				}
			}

			void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
			{
				sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
				sender.CoreWebView2.NavigationStarting += OnNavigationStarting;
				sender.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
				sender.CoreWebView2.ProcessFailed += OnProcessFailed;

				if (Handler is WebViewHandler handler)
				{
					sender.UpdateUserAgent(handler.VirtualView);
					if (sender.Source is not null)
					{
						handler.SyncPlatformCookies(sender.Source.ToString()).FireAndForget();
					}
				}
			}

			void OnHistoryChanged(CoreWebView2 sender, object args)
			{
				if (Handler is WebViewHandler handler)
				{
					handler.PlatformView?.UpdateCanGoBackForward(handler.VirtualView);
				}
			}

			void OnNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
			{
				if (Handler is WebViewHandler handler)
				{
					if (args.IsSuccess)
						handler.NavigationSucceeded(sender, args);
					else
						handler.NavigationFailed(sender, args);
				}
			}

			void OnNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
			{
				if (Handler is WebViewHandler handler)
				{
					handler.OnNavigationStarting(sender, args);
				}
			}

			void OnProcessFailed(CoreWebView2 sender, CoreWebView2ProcessFailedEventArgs args)
			{
				if (Handler is WebViewHandler handler)
				{
					handler.ProcessFailed(sender, args);
				}
			}
		}
	}
}
