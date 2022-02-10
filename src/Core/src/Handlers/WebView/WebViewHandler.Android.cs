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
		public const string AssetBaseUrl = "file:///android_asset/";

		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;

		bool _firstRun = true;
		readonly HashSet<string> _loadedCookies = new HashSet<string>();

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

			base.DisconnectHandler(nativeView);
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}
	
		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebViewClient(handler._webViewClient ??= new MauiWebViewClient(handler));
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
			handler.NativeView.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView.UpdateReload(webView);

			string? url = handler.NativeView.Url?.ToString();

			if (url == null)
				return;

			handler.SyncNativeCookies(url);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.NativeView?.Eval(webView, script);
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

		internal void SyncNativeCookiesToVirtualView(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);
			
			if (retrieveCurrentWebCookies == null)
				return;

			foreach (Cookie cookie in cookies)
			{
				var nativeCookie = retrieveCurrentWebCookies[cookie.Name];

				if (nativeCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = nativeCookie.Value;
			}

			SyncNativeCookies(url);
		}

		void SyncNativeCookies(string url)
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

			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);
			
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
				var existingCookies = GetCookiesFromNativeStore(url);

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

		CookieCollection? GetCookiesFromNativeStore(string url)
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

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				handler.NativeView.EvaluateJavaScript(request);
			}
		}
	}
}