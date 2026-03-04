#pragma warning disable CA1416 // Validate platform compatibility
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Web.Http;
using static System.String;
using WWebView = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class WebViewRenderer : ViewRenderer<WebView, WebView2>, IWebViewDelegate
	{
		IWebViewController WebViewController => Element;
		WebNavigationEvent _eventState;
		bool _updating;
		WebView2 _internalWebView;
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
			_internalWebView = new WebView2();

			// When the 'navigation' to the original HTML string is done, we can modify it to include our <base> tag
			_internalWebView.NavigationCompleted += async (sender, args) =>
			{
				// Generate a version of the <base> script with the correct <base> tag
				var script = BaseInsertionScript.Replace("baseTag", baseTag, StringComparison.Ordinal);

				// Run it and retrieve the updated HTML from our WebView
				await sender.ExecuteScriptAsync(script);
				htmlWithBaseTag = await sender.ExecuteScriptAsync("document.documentElement.outerHTML;");

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
					Control.Source = uri;

					// TODO WINUI3
					//var httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri);
					//Control.NavigateWithHttpRequestMessage(httpRequestMessage);
				}
				catch (Exception exc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<WebViewRenderer>()?.LogWarning(exc, "Failed to load: {uri}", uri);
				}
			}
			else
			{
				try
				{
					//No Cookies so just navigate...
					Control.Source = uri;
				}
				catch (Exception exc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<WebViewRenderer>()?.LogWarning(exc, "Failed to load: {uri}", uri);
				}
			}
		}

		void TearDown(WWebView webView)
		{
			if (webView == null)
			{
				return;
			}
			webView.NavigationStarting -= OnNavigationStarted;
			webView.NavigationCompleted -= OnNavigationCompleted;
			webView.CoreProcessFailed -= OnCoreProcessFailed;
			webView.WebMessageReceived -= OnWebMessageReceived;
		}

		void Connect(WWebView webView)
		{
			if (webView == null)
			{
				return;
			}

			webView.CoreProcessFailed += OnCoreProcessFailed;
			webView.WebMessageReceived += OnWebMessageReceived;
			webView.NavigationStarting += OnNavigationStarted;
			webView.NavigationCompleted += OnNavigationCompleted;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TearDown(Control);
				if (Element != null)
				{
					Control.NavigationStarting -= OnNavigationStarted;
					Control.NavigationCompleted -= OnNavigationCompleted;
					WebViewController.EvalRequested -= OnEvalRequested;
					WebViewController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
					WebViewController.GoBackRequested -= OnGoBackRequested;
					WebViewController.GoForwardRequested -= OnGoForwardRequested;
					WebViewController.ReloadRequested -= OnReloadRequested;
				}
			}

			base.Dispose(disposing);
		}

		protected virtual WWebView CreateNativeControl()
		{
			// TODO WINUI3
			//if (Element.IsSet(PlatformConfiguration.WindowsSpecific.WebView.ExecutionModeProperty))
			//{
			//	WWebViewExecutionMode webViewExecutionMode = WWebViewExecutionMode.SameThread;

			//	switch (Element.OnThisPlatform().GetExecutionMode())
			//	{
			//		case PlatformConfiguration.WindowsSpecific.WebViewExecutionMode.SameThread:
			//			webViewExecutionMode = WWebViewExecutionMode.SameThread;
			//			break;
			//		case PlatformConfiguration.WindowsSpecific.WebViewExecutionMode.SeparateProcess:
			//			webViewExecutionMode = WWebViewExecutionMode.SeparateProcess;
			//			break;
			//		case PlatformConfiguration.WindowsSpecific.WebViewExecutionMode.SeparateThread:
			//			webViewExecutionMode = WWebViewExecutionMode.SeparateThread;
			//			break;

			//	}

			//	return new WWebView(webViewExecutionMode);
			//}

			return new WWebView();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				IWebViewController oldElement = e.OldElement;
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
					var webView = CreateNativeControl();
					Connect(webView);
					SetNativeControl(webView);
				}

				IWebViewController newElement = e.NewElement;
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
			else if (e.Is(PlatformConfiguration.WindowsSpecific.WebView.ExecutionModeProperty))
			{
				UpdateExecutionMode();
			}
		}

		[PortHandler]
		HashSet<string> _loadedCookies = new HashSet<string>();

		[PortHandler]
		Uri CreateUriForCookies(string url)
		{
			if (url == null)
				return null;

			Uri uri;

			if (url.Length > 2000)
				url = url.Substring(0, 2000);

			if (Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				if (String.IsNullOrWhiteSpace(uri.Host))
					return null;

				return uri;
			}

			return null;
		}

		[PortHandler]
		HttpCookieCollection GetCookiesFromNativeStore(string url)
		{
			var uri = CreateUriForCookies(url);
			CookieContainer existingCookies = new CookieContainer();
			var filter = new global::Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var nativeCookies = filter.CookieManager.GetCookies(uri);
			return nativeCookies;
		}

		[PortHandler]
		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = new Uri(url);

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

		[PortHandler]
		void SyncNativeCookiesToElement(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

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

		[PortHandler]
		void SyncNativeCookies(string url)
		{
			var uri = CreateUriForCookies(url);
			if (uri == null)
				return;

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
			Element.Source?.Load(this);

			UpdateCanGoBackForward();
		}

		async void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			await Control.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				async () =>
				{
					try
					{
						await Control.ExecuteScriptAsync(eventArg.Script);
					}
					catch (Exception exc)
					{
						Application.Current?.FindMauiContext()?.CreateLogger<WebView>()?.LogWarning(exc, "Eval of script failed Script: {eventArg.Script}", eventArg.Script);
					}
				});
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			return await Control.ExecuteScriptAsync(script);
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
			Control.Reload();
		}

		[PortHandler]
		async void NavigationSucceeded(WWebView sender, Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			// TODO WINUI3
			//if (e.Uri != null)
			//	SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Success);
			Uri uri = sender.Source;
			if (uri != null)
				SendNavigated(new UrlWebViewSource { Url = uri.AbsoluteUri }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();

			if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
				await Control.ExecuteScriptAsync("window.alert = function(message){ window.external.notify(message); };");

		}

		[PortHandler]
		void NavigationFailed(WWebView sender, Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			Uri uri = sender.Source;
			if (uri != null)
				SendNavigated(new UrlWebViewSource { Url = uri.AbsoluteUri }, _eventState, WebNavigationResult.Failure);

		}

		void OnNavigationCompleted(WWebView sender, Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			if (e.IsSuccess)
				NavigationSucceeded(sender, e);
			else
				NavigationFailed(sender, e);
		}

		[PortHandler]
		async void OnWebMessageReceived(WWebView sender, Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
		{
			if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
				await new global::Windows.UI.Popups.MessageDialog(e.TryGetWebMessageAsString()).ShowAsync();
		}

		[PortHandler]
		void OnNavigationStarted(WWebView sender, Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
		{
			// TODO WINUI3
			//Uri uri = e.Uri;

			//if (uri != null)
			//{
			Uri uri;

			if (Uri.TryCreate(e.Uri, UriKind.Absolute, out uri) && uri != null)
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = uri.AbsoluteUri }, uri.AbsoluteUri);

				WebViewController.SendNavigating(args);
				e.Cancel = args.Cancel;

				// reset in this case because this is the last event we will get
				if (args.Cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
		}

		[PortHandler]
		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_updating = true;
			((IElementController)Element).SetValueFromRenderer(WebView.SourceProperty, source);
			_updating = false;

			SyncNativeCookiesToElement(source.Url);
			WebViewController.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}

		[PortHandler]
		void UpdateCanGoBackForward()
		{
			((IWebViewController)Element).CanGoBack = Control.CanGoBack;
			((IWebViewController)Element).CanGoForward = Control.CanGoForward;
		}

		void UpdateExecutionMode()
		{
			TearDown(Control);
			var webView = CreateNativeControl();
			Connect(webView);
			SetNativeControl(webView);
			Load();
		}

		void OnCoreProcessFailed(WWebView sender, Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs args)
		{
			UpdateExecutionMode();
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility