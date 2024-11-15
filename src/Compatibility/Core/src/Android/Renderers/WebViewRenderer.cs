using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Android.Content;
using Android.Webkit;
using System.Web;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using AWebView = Android.Webkit.WebView;
using MixedContentHandling = Android.Webkit.MixedContentHandling;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class WebViewRenderer : ViewRenderer<WebView, AWebView>, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		WebNavigationEvent _eventState;
		WebViewClient _webViewClient;
		FormsWebChromeClient _webChromeClient;
		bool _isDisposed = false;
		protected internal IWebViewController ElementController => Element;
		protected internal bool IgnoreSourceChanges { get; set; }
		protected internal string UrlCanceled { get; set; }

		public WebViewRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		public void LoadHtml(string html, string baseUrl)
		{
			_eventState = WebNavigationEvent.NewPage;
			Control.LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html, "text/html", "UTF-8", null);
		}

		public void LoadUrl(string url)
		{
			LoadUrl(url, true);
		}

		void LoadUrl(string url, bool fireNavigatingCanceled)
		{
			if (!fireNavigatingCanceled || !SendNavigatingCanceled(url))
			{
				_eventState = WebNavigationEvent.NewPage;
				
				string? encodedUrl = url;
				if (!string.IsNullOrEmpty(encodedUrl))
				{
					int questionMarkIndex = encodedUrl.IndexOf('?', StringComparison.InvariantCulture);

					if (questionMarkIndex != -1)
					{
						string baseUrl = encodedUrl.Substring(0, questionMarkIndex + 1);
						string queryString = encodedUrl.Substring(questionMarkIndex + 1);

						// URI encode the part after the '?'
						string encodedPart = HttpUtility.UrlEncode(queryString);
						encodedUrl = baseUrl + encodedPart;
					}
				}

				if (url != null && !url.StartsWith('/') && !Uri.IsWellFormedUriString(encodedUrl, UriKind.Absolute))
				{
					// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
					url = AssetBaseUrl + url;
				}
				Control.LoadUrl(url);
			}
		}

		protected internal bool SendNavigatingCanceled(string url)
		{
			if (Element == null || string.IsNullOrWhiteSpace(url))
				return true;

			if (url == AssetBaseUrl)
				return false;

			var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);
			SyncNativeCookies(url);
			ElementController.SendNavigating(args);
			UpdateCanGoBackForward();
			UrlCanceled = args.Cancel ? null : url;
			return args.Cancel;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;
			if (disposing)
			{
				if (Element != null)
				{
					Control?.StopLoading();

					ElementController.EvalRequested -= OnEvalRequested;
					ElementController.GoBackRequested -= OnGoBackRequested;
					ElementController.GoForwardRequested -= OnGoForwardRequested;
					ElementController.ReloadRequested -= OnReloadRequested;
					ElementController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;

					_webViewClient?.Dispose();
					_webChromeClient?.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		[PortHandler]
		protected virtual WebViewClient GetWebViewClient()
		{
			return new FormsWebViewClient(this);
		}

		[PortHandler]
		protected virtual FormsWebChromeClient GetFormsWebChromeClient()
		{
			return new FormsWebChromeClient();
		}

		protected override Size MinimumSize()
		{
			return new Size(Context.ToPixels(40), Context.ToPixels(40));
		}

		[PortHandler]
		protected override AWebView CreateNativeControl()
		{
			var webView = new AWebView(Context);
			webView.Settings.SetSupportMultipleWindows(true);
			return webView;
		}

		internal WebNavigationEvent GetCurrentWebNavigationEvent()
		{
			return _eventState;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var webView = CreateNativeControl();
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
#pragma warning disable CA1416, CA1422 // Validate platform compatibility
				webView.LayoutParameters = new global::Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0);
#pragma warning restore CA1416, CA1422 // Validate platform compatibility
#pragma warning restore 618

				_webViewClient = GetWebViewClient();
				webView.SetWebViewClient(_webViewClient);

				_webChromeClient = GetFormsWebChromeClient();
				_webChromeClient.SetContext(Context);
				webView.SetWebChromeClient(_webChromeClient);
				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				SetNativeControl(webView);
			}

			if (e.OldElement != null)
			{
				var oldElementController = e.OldElement as IWebViewController;
				oldElementController.EvalRequested -= OnEvalRequested;
				oldElementController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				oldElementController.GoBackRequested -= OnGoBackRequested;
				oldElementController.GoForwardRequested -= OnGoForwardRequested;
				oldElementController.ReloadRequested -= OnReloadRequested;
			}

			if (e.NewElement != null)
			{
				var newElementController = e.NewElement as IWebViewController;
				newElementController.EvalRequested += OnEvalRequested;
				newElementController.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
				newElementController.GoBackRequested += OnGoBackRequested;
				newElementController.GoForwardRequested += OnGoForwardRequested;
				newElementController.ReloadRequested += OnReloadRequested;

				UpdateMixedContentMode();
				UpdateEnableZoomControls();
				UpdateDisplayZoomControls();
			}

			Load();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Source":
					Load();
					break;
				case "MixedContentMode":
					UpdateMixedContentMode();
					break;
				case "EnableZoomControls":
					UpdateEnableZoomControls();
					break;
				case "DisplayZoomControls":
					UpdateDisplayZoomControls();
					break;
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
		CookieCollection GetCookiesFromNativeStore(string url)
		{
			CookieContainer existingCookies = new CookieContainer();
			var cookieManager = CookieManager.Instance;
			var currentCookies = cookieManager.GetCookie(url);
			var uri = CreateUriForCookies(url);

			if (currentCookies != null)
			{
				foreach (var cookie in currentCookies.Split(';'))
					existingCookies.SetCookies(uri, cookie);
			}

			return existingCookies.GetCookies(uri);
		}

		[PortHandler]
		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = Element.Cookies;
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
				foreach (Cookie cookie in existingCookies)
				{
					if (cookies[cookie.Name] == null)
						myCookieJar.Add(cookie);
				}
			}
		}

		[PortHandler]
		internal void SyncNativeCookiesToElement(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);
			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

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

			var cookieManager = CookieManager.Instance;
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

		void Load()
		{
			if (IgnoreSourceChanges)
				return;

			Element.Source?.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			LoadUrl("javascript:" + eventArg.Script, false);
		}

		Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var jsr = new JavascriptResult();

			Control.EvaluateJavascript(script, jsr);

			return jsr.JsResult;
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack())
			{
				_eventState = WebNavigationEvent.Back;
				Control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward())
			{
				_eventState = WebNavigationEvent.Forward;
				Control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			SyncNativeCookies(Control.Url?.ToString());
			_eventState = WebNavigationEvent.Refresh;
			Control.Reload();
		}

		[PortHandler]
		protected internal void UpdateCanGoBackForward()
		{
			if (Element == null || Control == null)
				return;
			ElementController.CanGoBack = Control.CanGoBack();
			ElementController.CanGoForward = Control.CanGoForward();
		}

		[PortHandler]
		void UpdateMixedContentMode()
		{
			if (Control != null)
			{
				Control.Settings.MixedContentMode = (MixedContentHandling)Element.OnThisPlatform().MixedContentMode();
			}
		}

		[PortHandler]
		void UpdateEnableZoomControls()
		{
			var value = Element.OnThisPlatform().ZoomControlsEnabled();
			Control.Settings.SetSupportZoom(value);
			Control.Settings.BuiltInZoomControls = value;
		}

		[PortHandler]
		void UpdateDisplayZoomControls()
		{
			Control.Settings.DisplayZoomControls = Element.OnThisPlatform().ZoomControlsDisplayed();
		}

		class JavascriptResult : Java.Lang.Object, IValueCallback
		{
			TaskCompletionSource<string> source;
			public Task<string> JsResult { get { return source.Task; } }

			public JavascriptResult()
			{
				source = new TaskCompletionSource<string>();
			}

			public void OnReceiveValue(Java.Lang.Object result)
			{
				string json = ((Java.Lang.String)result).ToString();
				source.SetResult(json);
			}
		}
	}
}