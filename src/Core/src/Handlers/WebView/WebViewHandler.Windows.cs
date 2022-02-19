using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Web.Http;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		readonly HashSet<string> _loadedCookies = new HashSet<string>();

		protected override WebView2 CreatePlatformView() => new MauiWebView();

		protected override void ConnectHandler(WebView2 platformView)
		{
			platformView.NavigationStarting += OnNavigationStarted;
			platformView.NavigationCompleted += OnNavigationCompleted;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
			platformView.NavigationStarting -= OnNavigationStarted;
			platformView.NavigationCompleted -= OnNavigationCompleted;

			base.DisconnectHandler(platformView);
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;

			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateReload(webView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		void OnNavigationStarted(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
		{
			// TODO: Notify navigation state.
		}
		
		void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			if (args.IsSuccess)
				NavigationSucceeded(sender, args);
			else
				NavigationFailed(sender, args);
		}

		void NavigationSucceeded(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			string? url = sender.Source?.ToString();

			if (url == null)
				return;

			SendNavigated(url);

			if (VirtualView == null)
				return;

			sender.UpdateCanGoBackForward(VirtualView);
		}

		void NavigationFailed(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			string? url = sender.Source?.ToString();

			if (url == null)
				return;

			SendNavigated(url);
		}
		
		void SendNavigated(string url)
		{
			SyncPlatformCookiesToVirtualView(url);

			PlatformView?.UpdateCanGoBackForward(VirtualView);
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

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg) 
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