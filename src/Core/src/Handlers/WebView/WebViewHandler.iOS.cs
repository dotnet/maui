using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using WebKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WKWebView>
	{
		readonly HashSet<string> _loadedCookies = new HashSet<string>();

		protected virtual float MinimumSize => 44f;

		WKUIDelegate? _delegate;

		protected override WKWebView CreatePlatformView() =>
			new MauiWKWebView(this);

		public static void MapWKUIDelegate(IWebViewHandler handler, IWebView webView)
		{
			if (handler is WebViewHandler platformHandler)
			{
				handler.PlatformView.UIDelegate = platformHandler._delegate ??= new MauiWebViewUIDelegate(handler);
			}
		}

		static void MapBackground(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView.Opaque = webView.Background is null;
			handler.PlatformView.UpdateBackground(webView);
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
			if (handler.PlatformView.CanGoBack && handler.PlatformView.NavigationDelegate is MauiWebViewNavigationDelegate mauiDelegate)
				mauiDelegate.CurrentNavigationEvent = WebNavigationEvent.Back;

			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.PlatformView.CanGoForward && handler.PlatformView.NavigationDelegate is MauiWebViewNavigationDelegate mauiDelegate)
				mauiDelegate.CurrentNavigationEvent = WebNavigationEvent.Forward;

			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static async void MapReload(IWebViewHandler handler, IWebView webView, object? arg)
		{
			var platformHandler = handler as WebViewHandler;
			if (platformHandler == null)
			{
				return;
			}

			try
			{
				var url = ((MauiWKWebView)handler.PlatformView).CurrentUrl;

				if (url != null)
					await platformHandler.SyncPlatformCookiesAsync(url);
			}
			catch (Exception exc)
			{
				handler.MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning(exc, "Syncing Existing Cookies Failed");
			}

			if (handler.PlatformView.NavigationDelegate is MauiWebViewNavigationDelegate mauiDelegate)
				mauiDelegate.CurrentNavigationEvent = WebNavigationEvent.Refresh;

			handler.PlatformView?.UpdateReload(webView);
		}

		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);

			var set = false;

			var width = size.Width;
			var height = size.Height;

			if (width == 0)
			{
				if (widthConstraint <= 0 || double.IsInfinity(widthConstraint))
				{
					width = MinimumSize;
					set = true;
				}
			}

			if (height == 0)
			{
				if (heightConstraint <= 0 || double.IsInfinity(heightConstraint))
				{
					height = MinimumSize;
					set = true;
				}
			}

			if (set)
				size = new Size(width, height);

			return size;
		}

		internal async Task ProcessNavigatedAsync(string url)
		{
			if (VirtualView == null)
				return;

			try
			{
				if (VirtualView.Cookies != null)
					await SyncPlatformCookiesToVirtualViewAsync(url);
			}
			catch
			{
				MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning("Failed to Sync Cookies");
			}

			PlatformView?.UpdateCanGoBackForward(VirtualView);
		}

		internal async Task FirstLoadUrlAsync(string url)
		{
			try
			{
				var uri = new Uri(url);

				var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
				var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
				var request = new NSUrlRequest(new NSUrl(new Uri(safeHostUri, safeRelativeUri).AbsoluteUri));

				if (HasCookiesToLoad(url) && !(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
					return;

				await SyncPlatformCookiesAsync(url);
				PlatformView?.LoadRequest(request);
			}
			catch (UriFormatException)
			{
				// If we got a format exception trying to parse the URI, it might be because
				// someone is passing in a local bundled file page. If we can find a better way
				// to detect that scenario, we should use it; until then, we'll fall back to 
				// local file loading here and see if that works:
				if (!LoadFile(url))
				{
					MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning($"Unable to Load Url {url}");
				}
			}
			catch (Exception)
			{
				MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning($"Unable to Load Url {url}");
			}
		}

		internal bool HasCookiesToLoad(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri == null)
				return false;

			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return false;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies == null)
				return false;

			return cookies.Count > 0;
		}

		internal async Task SyncPlatformCookiesToVirtualViewAsync(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return;

			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = await GetCookiesFromPlatformStore(url);

			foreach (var nscookie in retrieveCurrentWebCookies)
			{
				if (cookies[nscookie.Name] == null)
				{
					string cookieH = $"{nscookie.Name}={nscookie.Value}; domain={nscookie.Domain}; path={nscookie.Path}";

					myCookieJar.SetCookies(uri, cookieH);
				}
			}

			foreach (Cookie cookie in cookies)
			{
				NSHttpCookie? nSHttpCookie = null;

				foreach (var findCookie in retrieveCurrentWebCookies)
				{
					if (findCookie.Name == cookie.Name)
					{
						nSHttpCookie = findCookie;
						break;
					}
				}

				if (nSHttpCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = nSHttpCookie.Value;
			}

			await SyncPlatformCookiesAsync(url);
		}

		internal async Task SyncPlatformCookiesAsync(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			await InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);
			if (cookies == null)
				return;

			var retrieveCurrentWebCookies = await GetCookiesFromPlatformStore(url);

			List<NSHttpCookie> deleteCookies = new List<NSHttpCookie>();
			foreach (var cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] != null)
					continue;

				deleteCookies.Add(cookie);
			}

			List<Cookie> cookiesToSet = new List<Cookie>();
			foreach (Cookie cookie in cookies)
			{
				bool changeCookie = true;

				// This code is used to only push updates to cookies that have changed.
				// This doesn't quite work on on iOS 10 if we have to delete any cookies.
				// I haven't found a way on iOS 10 to remove individual cookies. 
				// The trick we use on Android with writing a cookie that expires doesn't work
				// So on iOS10 if the user wants to remove any cookies we just delete 
				// the cookie for the entire domain inside of DeleteCookies and then rewrite
				// all the cookies
				if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11) || deleteCookies.Count == 0)
				{
					foreach (var nsCookie in retrieveCurrentWebCookies)
					{
						// if the cookie value hasn't changed don't set it again
						if (nsCookie.Domain == cookie.Domain &&
							nsCookie.Name == cookie.Name &&
							nsCookie.Value == cookie.Value)
						{
							changeCookie = false;
							break;
						}
					}
				}

				if (changeCookie)
					cookiesToSet.Add(cookie);
			}

			await SetCookie(cookiesToSet);
			await DeleteCookies(deleteCookies);
		}

		async Task InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = VirtualView.Cookies;

			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			if (!_loadedCookies.Add(uri.Host))
				return;

			// Pre ios 11 we sync cookies after navigated
			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var existingCookies = await GetCookiesFromPlatformStore(url);
			foreach (var nscookie in existingCookies)
			{
				if (cookies[nscookie.Name] == null)
				{
					string cookieH = $"{nscookie.Name}={nscookie.Value}; domain={nscookie.Domain}; path={nscookie.Path}";
					myCookieJar.SetCookies(uri, cookieH);
				}
			}
		}

		async Task<List<NSHttpCookie>> GetCookiesFromPlatformStore(string url)
		{
			NSHttpCookie[]? _initialCookiesLoaded = null;

			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				_initialCookiesLoaded = await PlatformView.Configuration.WebsiteDataStore.HttpCookieStore.GetAllCookiesAsync();
			}
			else
			{
				// TODO: Implement EvaluateJavaScriptAsync.
			}

			_initialCookiesLoaded ??= Array.Empty<NSHttpCookie>();

			List<NSHttpCookie> existingCookies = new List<NSHttpCookie>();

			var uriForCookies = CreateUriForCookies(url);

			if (uriForCookies != null)
			{
				string domain = uriForCookies.Host;
				foreach (var cookie in _initialCookiesLoaded)
				{
					// we don't care that much about this being accurate
					// the cookie container will split the cookies up more correctly
					if (!cookie.Domain.Contains(domain, StringComparison.Ordinal) && !domain.Contains(cookie.Domain, StringComparison.Ordinal))
						continue;

					existingCookies.Add(cookie);
				}
			}

			return existingCookies;
		}

		async Task SetCookie(List<Cookie> cookies)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				foreach (var cookie in cookies)
					await PlatformView.Configuration.WebsiteDataStore.HttpCookieStore.SetCookieAsync(new NSHttpCookie(cookie));
			}
			else
			{
				PlatformView.Configuration.UserContentController.RemoveAllUserScripts();

				if (cookies.Count > 0)
				{
					WKUserScript wKUserScript = new WKUserScript(new NSString(GetCookieString(cookies)), WKUserScriptInjectionTime.AtDocumentStart, false);

					PlatformView.Configuration.UserContentController.AddUserScript(wKUserScript);
				}
			}
		}

		async Task DeleteCookies(List<NSHttpCookie> cookies)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				foreach (var cookie in cookies)
					await PlatformView.Configuration.WebsiteDataStore.HttpCookieStore.DeleteCookieAsync(cookie);
			}
			else
			{
				var wKWebsiteDataStore = WKWebsiteDataStore.DefaultDataStore;

				// This is the only way I've found to delete cookies on pre ios 11
				// I tried to set an expired cookie but it doesn't delete the cookie
				// So, just deleting the whole domain is the best option I've found
				WKWebsiteDataStore
					.DefaultDataStore
					.FetchDataRecordsOfTypes(WKWebsiteDataStore.AllWebsiteDataTypes, (NSArray records) =>
					{
						for (nuint i = 0; i < records.Count; i++)
						{
							var record = records.GetItem<WKWebsiteDataRecord>(i);

							foreach (var deleteme in cookies)
							{
								if (record.DisplayName.Contains(deleteme.Domain, StringComparison.Ordinal) || deleteme.Domain.Contains(record.DisplayName, StringComparison.Ordinal))
								{
									WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(record.DataTypes,
										  new[] { record }, () => { });

									break;
								}

							}
						}
					});
			}
		}

		static string GetCookieString(List<Cookie> existingCookies)
		{
			StringBuilder cookieBuilder = new StringBuilder();
			foreach (Cookie jCookie in existingCookies)
			{
				cookieBuilder.Append("document.cookie = '");
				cookieBuilder.Append(jCookie.Name);
				cookieBuilder.Append('=');

				if (jCookie.Expired)
				{
					cookieBuilder.Append($"; Max-Age=0");
					cookieBuilder.Append($"; expires=Sun, 31 Dec 2000 00:00:00 UTC");
				}
				else
				{
					cookieBuilder.Append(jCookie.Value);
					cookieBuilder.Append($"; Max-Age={jCookie.Expires.Subtract(DateTime.UtcNow).TotalSeconds}");
				}

				if (!String.IsNullOrWhiteSpace(jCookie.Domain))
				{
					cookieBuilder.Append($"; Domain={jCookie.Domain}");
				}
				if (!String.IsNullOrWhiteSpace(jCookie.Domain))
				{
					cookieBuilder.Append($"; Path={jCookie.Path}");
				}
				if (jCookie.Secure)
				{
					cookieBuilder.Append($"; Secure");
				}
				if (jCookie.HttpOnly)
				{
					cookieBuilder.Append($"; HttpOnly");
				}

				cookieBuilder.Append("';");
			}

			return cookieBuilder.ToString();
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

		bool LoadFile(string url)
		{
			try
			{
				var file = Path.GetFileNameWithoutExtension(url);
				var ext = Path.GetExtension(url);

				var nsUrl = NSBundle.MainBundle.GetUrlForResource(file, ext);

				if (nsUrl == null)
				{
					return false;
				}

				PlatformView?.LoadFileUrl(nsUrl, nsUrl);

				return true;
			}
			catch (Exception)
			{
				MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning("Could not load {url} as local file", url);
			}

			return false;
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