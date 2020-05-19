using System;
using System.ComponentModel;
using global::Windows.UI.Core;
using global::Windows.UI.Xaml.Controls;
using System.Maui.Internals;
using static System.String;
using System.Maui.PlatformConfiguration.WindowsSpecific;
using System.Threading.Tasks;
using System.Net;
using global::Windows.Web.Http;
using System.Collections.Generic;
using System.Linq;

namespace System.Maui.Platform.UWP
{
	public class WebViewRenderer : ViewRenderer<WebView, global::Windows.UI.Xaml.Controls.WebView>, IWebViewDelegate
	{
		WebNavigationEvent _eventState;
		bool _updating;
		global::Windows.UI.Xaml.Controls.WebView _internalWebView;
		const string LocalScheme = "ms-appx-web:///";

		// Script to insert a <base> tag into an HTML document
		const string BaseInsertionScript = @"
var head = document.getElementsByTagName('head')[0];
var bases = head.getElementsByTagName('base');
if(bases.length == 0){
    head.innerHTML = 'baseTag' + head.innerHTML;
}";
		public void LoadHtml(string html, string baseUrl)
		{
			if (IsNullOrEmpty(baseUrl))
			{
				baseUrl = LocalScheme;
			}

			// Generate a base tag for the document
			var baseTag = $"<base href=\"{baseUrl}\"></base>";

			string htmlWithBaseTag;

			// Set up an internal WebView we can use to load and parse the original HTML string
			// Make _internalWebView a field instead of local variable to avoid garbage collection
			_internalWebView = new global::Windows.UI.Xaml.Controls.WebView();

			// When the 'navigation' to the original HTML string is done, we can modify it to include our <base> tag
			_internalWebView.NavigationCompleted += async (sender, args) =>
			{
				// Generate a version of the <base> script with the correct <base> tag
				var script = BaseInsertionScript.Replace("baseTag", baseTag);

				// Run it and retrieve the updated HTML from our WebView
				await sender.InvokeScriptAsync("eval", new[] { script });
				htmlWithBaseTag = await sender.InvokeScriptAsync("eval", new[] { "document.documentElement.outerHTML;" });

				// Set the HTML for the 'real' WebView to the updated HTML
				Control.NavigateToString(!IsNullOrEmpty(htmlWithBaseTag) ? htmlWithBaseTag : html);
				// free up memory after we're done with _internalWebView
				_internalWebView = null;
			};

			// Kick off the initial navigation
			_internalWebView.NavigateToString(html);
		}

		public void LoadUrl(string url)
		{
			Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(LocalScheme + url, UriKind.RelativeOrAbsolute);
			}

			var cookies = Element.Cookies?.GetCookies(uri);
			if (cookies != null)
			{
				SyncNativeCookies(url);

				try
				{
					var httpRequestMessage = new global::Windows.Web.Http.HttpRequestMessage(global::Windows.Web.Http.HttpMethod.Get, uri);
					Control.NavigateWithHttpRequestMessage(httpRequestMessage);
				}
				catch (System.Exception exc)
				{
					Internals.Log.Warning(nameof(WebViewRenderer), $"Failed to load: {uri} {exc}");
				}
			}
			else
			{
				try
				{
					//No Cookies so just navigate...
					Control.Source = uri;
				}
				catch (System.Exception exc)
				{
					Internals.Log.Warning(nameof(WebViewRenderer), $"Failed to load: {uri} {exc}");
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.NavigationStarting -= OnNavigationStarted;
					Control.NavigationCompleted -= OnNavigationCompleted;
					Control.NavigationFailed -= OnNavigationFailed;
					Control.ScriptNotify -= OnScriptNotify;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var oldElement = e.OldElement;
				oldElement.EvalRequested -= OnEvalRequested;
				oldElement.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				oldElement.GoBackRequested -= OnGoBackRequested;
				oldElement.GoForwardRequested -= OnGoForwardRequested;
				oldElement.ReloadRequested -= OnReloadRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var webView = new global::Windows.UI.Xaml.Controls.WebView();
					webView.NavigationStarting += OnNavigationStarted;
					webView.NavigationCompleted += OnNavigationCompleted;
					webView.NavigationFailed += OnNavigationFailed;
					webView.ScriptNotify += OnScriptNotify;
					SetNativeControl(webView);
				}

				var newElement = e.NewElement;
				newElement.EvalRequested += OnEvalRequested;
				newElement.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
				newElement.GoForwardRequested += OnGoForwardRequested;
				newElement.GoBackRequested += OnGoBackRequested;
				newElement.ReloadRequested += OnReloadRequested;

				Load();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == WebView.SourceProperty.PropertyName)
			{
				if (!_updating)
					Load();
			}
		}

		HashSet<string> _loadedCookies = new HashSet<string>();

		HttpCookieCollection GetCookiesFromNativeStore(string url)
		{
			CookieContainer existingCookies = new CookieContainer();
			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var uri = new Uri(url);
			var nativeCookies = filter.CookieManager.GetCookies(uri);
			return nativeCookies;
		}

		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = new System.Uri(url);

			if (!_loadedCookies.Add(uri.Host))
				return;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies != null)
			{
				var existingCookies = GetCookiesFromNativeStore(url);
				foreach (HttpCookie cookie in existingCookies)
				{
					if (cookies[cookie.Name] == null)
						myCookieJar.SetCookies(uri, cookie.ToString());
				}
			}
		}

		void SyncNativeCookiesToElement(string url)
		{
			if (String.IsNullOrWhiteSpace(url))
				return;

			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = new Uri(url);
			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var nativeCookies = filter.CookieManager.GetCookies(uri);

			foreach (Cookie cookie in cookies)
			{
				var httpCookie = nativeCookies
					.FirstOrDefault(x => x.Name == cookie.Name);

				if (httpCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = httpCookie.Value;
			}

			SyncNativeCookies(url);
		}

		void SyncNativeCookies(string url)
		{
			if (String.IsNullOrWhiteSpace(url))
				return;

			var uri = new Uri(url);
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);
			if (cookies == null)
				return;

			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			foreach (Cookie cookie in cookies)
			{
				HttpCookie httpCookie = new HttpCookie(cookie.Name, cookie.Domain, cookie.Path);
				httpCookie.Value = cookie.Value;
				filter.CookieManager.SetCookie(httpCookie, false);
			}

			foreach (HttpCookie cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] != null)
					continue;

				filter.CookieManager.DeleteCookie(cookie);
			}
		}

		void Load()
		{
			if (Element.Source != null)
				Element.Source.Load(this);

			UpdateCanGoBackForward();
		}

		async void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			await Control.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await Control.InvokeScriptAsync("eval", new[] { eventArg.Script }));
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			return await Control.InvokeScriptAsync("eval", new[] { script });
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack)
			{
				_eventState = WebNavigationEvent.Back;
				Control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward)
			{
				_eventState = WebNavigationEvent.Forward;
				Control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			SyncNativeCookies(Control?.Source?.ToString());
			Control.Refresh();
		}

		async void OnNavigationCompleted(global::Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationCompletedEventArgs e)
		{
			if (e.Uri != null)
				SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();

			if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
				await Control.InvokeScriptAsync("eval", new string[] { "window.alert = function(message){ window.external.notify(message); };" });
		}

		void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
		{
			if (e.Uri != null)
				SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Failure);
		}

		void OnNavigationStarted(global::Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs e)
		{
			Uri uri = e.Uri;

			if (uri != null)
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = uri.AbsoluteUri }, uri.AbsoluteUri);

				Element.SendNavigating(args);
				e.Cancel = args.Cancel;

				// reset in this case because this is the last event we will get
				if (args.Cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
		}

		async void OnScriptNotify(object sender, NotifyEventArgs e)
		{
			if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
				await new global::Windows.UI.Popups.MessageDialog(e.Value).ShowAsync();
		}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_updating = true;
			((IElementController)Element).SetValueFromRenderer(WebView.SourceProperty, source);
			_updating = false;

			SyncNativeCookiesToElement(source.Url);
			Element.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}

		void UpdateCanGoBackForward()
		{
			((IWebViewController)Element).CanGoBack = Control.CanGoBack;
			((IWebViewController)Element).CanGoForward = Control.CanGoForward;
		}
	}
}
