using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using WebKit;
using PreserveAttribute = Foundation.PreserveAttribute;
using Uri = System.Uri;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class WkWebViewRenderer : WKWebView, IVisualElementRenderer, IWebViewDelegate, IEffectControlProvider, ITabStop
	{
		EventTracker _events;
		bool _ignoreSourceChanges;
		WebNavigationEvent _lastBackForwardEvent;
		VisualElementPackager _packager;
#pragma warning disable 0414
		VisualElementTracker _tracker;
#pragma warning restore 0414

		static WKProcessPool _sharedPool;
		bool _disposed;
		static int _sharedPoolCount = 0;
		static bool _firstLoadFinished = false;
		string _pendingUrl;
		IWebViewController WebViewController => WebView;

		[Preserve(Conditional = true)]
		public WkWebViewRenderer() : this(CreateConfiguration())
		{

		}


		[Preserve(Conditional = true)]
		public WkWebViewRenderer(WKWebViewConfiguration config) : base(CoreGraphics.CGRect.Empty, config)
		{

		}

		// https://developer.apple.com/forums/thread/99674
		// WKWebView and making sure cookies synchronize is really quirky
		// The main workaround I've found for ensuring that cookies synchronize 
		// is to share the Process Pool between all WkWebView instances.
		// It also has to be shared at the point you call init
		static WKWebViewConfiguration CreateConfiguration()
		{
			var config = new WKWebViewConfiguration();
			if (_sharedPool == null)
			{
				_sharedPool = config.ProcessPool;
			}
			else
			{
				config.ProcessPool = _sharedPool;
			}
			return config;
		}

		WebView WebView => Element as WebView;

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= HandlePropertyChanged;
			}

			if (element != null)
			{
				Element = element;
				Element.PropertyChanged += HandlePropertyChanged;

				if (_packager == null)
				{
					WebViewController.EvalRequested += OnEvalRequested;
					WebViewController.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
					WebViewController.GoBackRequested += OnGoBackRequested;
					WebViewController.GoForwardRequested += OnGoForwardRequested;
					WebViewController.ReloadRequested += OnReloadRequested;
					NavigationDelegate = new CustomWebViewNavigationDelegate(this);
					UIDelegate = new CustomWebViewUIDelegate();

					BackgroundColor = UIColor.Clear;

					AutosizesSubviews = true;

					_tracker = new VisualElementTracker(this);

					_packager = new VisualElementPackager(this);
					_packager.Load();
				}

				Load();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				AccessibilityIdentifier = Element.AutomationId;

			element?.SendViewInitialized(this);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public void LoadHtml(string html, string baseUrl)
		{
			if (html != null)
				LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_firstLoadFinished = true;
			if (!string.IsNullOrWhiteSpace(_pendingUrl))
			{
				var closure = _pendingUrl;
				_pendingUrl = null;

				// I realize this looks like the worst hack ever but iOS 11 and cookies are super quirky
				// and this is the only way I could figure out how to get iOS 11 to inject a cookie 
				// the first time a WkWebView is used in your app. This only has to run the first time a WkWebView is used 
				// anywhere in the application. All subsequents uses of WkWebView won't hit this hack
				// Even if it's a WkWebView on a new page.
				// read through this thread https://developer.apple.com/forums/thread/99674
				// Or Bing "WkWebView and Cookies" to see the myriad of hacks that exist
				// Most of them all came down to different variations of synching the cookies before or after the
				// WebView is added to the controller. This is the only one I was able to make work
				// I think if we could delay adding the WebView to the Controller until after ViewWillAppear fires that might also work
				// But we're not really setup for that
				// If you'd like to try your hand at cleaning this up then UI Test Issue12134 and Issue3262 are your final bosses
				InvokeOnMainThread(async () =>
				{
					await Task.Delay(500);
					LoadUrl(closure);
				});
			}
		}

		[PortHandler]
		public async void LoadUrl(string url)
		{
			try
			{

				var uri = new Uri(url);

				var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
				var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
				NSUrlRequest request = new NSUrlRequest(new Uri(safeHostUri, safeRelativeUri));

				if (!_firstLoadFinished && HasCookiesToLoad(url) && !Forms.IsiOS13OrNewer)
				{
					_pendingUrl = url;
					return;
				}

				_firstLoadFinished = true;
				await SyncNativeCookies(url);
				LoadRequest(request);
			}
			catch (UriFormatException formatException)
			{
				// If we got a format exception trying to parse the URI, it might be because
				// someone is passing in a local bundled file page. If we can find a better way
				// to detect that scenario, we should use it; until then, we'll fall back to 
				// local file loading here and see if that works:
				if (!LoadFile(url))
				{
					Forms.MauiContext?.CreateLogger<WkWebViewRenderer>()?.LogWarning(formatException, "Unable to Load Url {url} ", url);
				}
			}
			catch (Exception exc)
			{
				Forms.MauiContext?.CreateLogger<WkWebViewRenderer>()?.LogWarning(exc, "Unable to Load Url {url}", url);
			}
		}

		[PortHandler]
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

				LoadFileUrl(nsUrl, nsUrl);

				return true;
			}
			catch (Exception ex)
			{
				Forms.MauiContext?.CreateLogger<WkWebViewRenderer>()?.LogWarning(ex, "Could not load {url} as local file", url);
			}

			return false;
		}

		[PortHandler]
		bool HasCookiesToLoad(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri == null)
				return false;

			var myCookieJar = WebView.Cookies;
			if (myCookieJar == null)
				return false;

			var cookies = myCookieJar.GetCookies(uri);
			if (cookies == null)
				return false;

			return cookies.Count > 0;
		}

		[PortHandler]
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// ensure that inner scrollview properly resizes when frame of webview updated
			ScrollView.Frame = Bounds;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (Interlocked.Decrement(ref _sharedPoolCount) == 0 && Forms.IsiOS12OrNewer)
				_sharedPool = null;

			if (disposing)
			{
				if (IsLoading)
					StopLoading();

				Element.PropertyChanged -= HandlePropertyChanged;
				WebViewController.EvalRequested -= OnEvalRequested;
				WebViewController.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				WebViewController.GoBackRequested -= OnGoBackRequested;
				WebViewController.GoForwardRequested -= OnGoForwardRequested;
				WebViewController.ReloadRequested -= OnReloadRequested;

				Element?.ClearValue(Platform.RendererProperty);
				SetElement(null);

				_events?.Dispose();
				_tracker?.Dispose();
				_packager?.Dispose();

				_events = null;
				_tracker = null;
				_events = null;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e) =>
			ElementChanged?.Invoke(this, e);

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
		async Task<List<NSHttpCookie>> GetCookiesFromNativeStore(string url)
		{
			NSHttpCookie[] _initialCookiesLoaded = null;
			if (Forms.IsiOS11OrNewer)
			{
				_initialCookiesLoaded = await Configuration.WebsiteDataStore.HttpCookieStore.GetAllCookiesAsync();
			}
			else
			{
				// I haven't found a different way to get the cookies pre ios 11
				var cookieString = await WebView.EvaluateJavaScriptAsync("document.cookie");

				if (cookieString != null)
				{
					CookieContainer extractCookies = new CookieContainer();
					var uri = CreateUriForCookies(url);

					foreach (var cookie in cookieString.Split(';'))
						extractCookies.SetCookies(uri, cookie);

					var extracted = extractCookies.GetCookies(uri);
					_initialCookiesLoaded = new NSHttpCookie[extracted.Count];
					for (int i = 0; i < extracted.Count; i++)
					{
						_initialCookiesLoaded[i] = new NSHttpCookie(extracted[i]);
					}
				}
			}

			_initialCookiesLoaded = _initialCookiesLoaded ?? Array.Empty<NSHttpCookie>();

			List<NSHttpCookie> existingCookies = new List<NSHttpCookie>();
			string domain = CreateUriForCookies(url).Host;
			foreach (var cookie in _initialCookiesLoaded)
			{
				// we don't care that much about this being accurate
				// the cookie container will split the cookies up more correctly
				if (!cookie.Domain.Contains(domain, StringComparison.Ordinal) && !domain.Contains(cookie.Domain, StringComparison.Ordinal))
					continue;

				existingCookies.Add(cookie);
			}

			return existingCookies;
		}

		[PortHandler]
		async Task InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = WebView.Cookies;
			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			if (!_loadedCookies.Add(uri.Host))
				return;

			// pre ios 11 we sync cookies after navigated
			if (!Forms.IsiOS11OrNewer)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var existingCookies = await GetCookiesFromNativeStore(url);
			foreach (var nscookie in existingCookies)
			{
				if (cookies[nscookie.Name] == null)
				{
					string cookieH = $"{nscookie.Name}={nscookie.Value}; domain={nscookie.Domain}; path={nscookie.Path}";
					myCookieJar.SetCookies(uri, cookieH);
				}
			}
		}

		[PortHandler]
		internal async Task SyncNativeCookiesToElement(string url)
		{
			if (String.IsNullOrWhiteSpace(url))
				return;

			var myCookieJar = WebView.Cookies;
			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);
			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = await GetCookiesFromNativeStore(url);

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
				NSHttpCookie nSHttpCookie = null;

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

			await SyncNativeCookies(url);
		}

		[PortHandler]
		async Task SyncNativeCookies(string url)
		{
			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var myCookieJar = WebView.Cookies;
			if (myCookieJar == null)
				return;

			await InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);
			if (cookies == null)
				return;

			var retrieveCurrentWebCookies = await GetCookiesFromNativeStore(url);

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
				if (Forms.IsiOS11OrNewer || deleteCookies.Count == 0)
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

		[PortHandler]
		async Task SetCookie(List<Cookie> cookies)
		{
			if (Forms.IsiOS11OrNewer)
			{
				foreach (var cookie in cookies)
					await Configuration.WebsiteDataStore.HttpCookieStore.SetCookieAsync(new NSHttpCookie(cookie));
			}
			else
			{
				Configuration.UserContentController.RemoveAllUserScripts();

				if (cookies.Count > 0)
				{
					WKUserScript wKUserScript = new WKUserScript(new NSString(GetCookieString(cookies)), WKUserScriptInjectionTime.AtDocumentStart, false);

					Configuration.UserContentController.AddUserScript(wKUserScript);
				}
			}
		}

		[PortHandler]
		async Task DeleteCookies(List<NSHttpCookie> cookies)
		{
			if (Forms.IsiOS11OrNewer)
			{
				foreach (var cookie in cookies)
					await Configuration.WebsiteDataStore.HttpCookieStore.DeleteCookieAsync(cookie);
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

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == WebView.SourceProperty.PropertyName)
				Load();
		}

		void Load()
		{
			if (_ignoreSourceChanges)
				return;

			((WebView)Element).Source?.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			EvaluateJavaScriptAsync(eventArg.Script);
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var result = await EvaluateJavaScriptAsync(script);
			return result?.ToString();
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (CanGoBack)
			{
				_lastBackForwardEvent = WebNavigationEvent.Back;
				GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (CanGoForward)
			{
				_lastBackForwardEvent = WebNavigationEvent.Forward;
				GoForward();
			}

			UpdateCanGoBackForward();
		}

		[PortHandler]
		async void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			try
			{

				await SyncNativeCookies(Url?.AbsoluteUrl?.ToString());
			}
			catch (Exception exc)
			{
				Forms.MauiContext?.CreateLogger<WkWebViewRenderer>()?.LogWarning(exc, "Syncing Existing Cookies Failed");
			}

			Reload();
		}

		[PortHandler]
		void UpdateCanGoBackForward()
		{
			((IWebViewController)WebView).CanGoBack = CanGoBack;
			((IWebViewController)WebView).CanGoForward = CanGoForward;
		}

		[PortHandler]
		string GetCookieString(List<Cookie> existingCookies)
		{
			StringBuilder cookieBuilder = new StringBuilder();
			foreach (System.Net.Cookie jCookie in existingCookies)
			{
				cookieBuilder.Append("document.cookie = '");
				cookieBuilder.Append(jCookie.Name);
				cookieBuilder.Append("=");

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

		class CustomWebViewNavigationDelegate : WKNavigationDelegate
		{
			readonly WkWebViewRenderer _renderer;
			WebNavigationEvent _lastEvent;

			public CustomWebViewNavigationDelegate(WkWebViewRenderer renderer)
			{
				if (renderer == null)
					throw new ArgumentNullException("renderer");
				_renderer = renderer;
			}

			WebView WebView => _renderer.WebView;
			IWebViewController WebViewController => WebView;

			[PortHandler]
			public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				var url = GetCurrentUrl();
				WebViewController.SendNavigated(
					new WebNavigatedEventArgs(_lastEvent, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
				);

				_renderer.UpdateCanGoBackForward();
			}

			[PortHandler]
			public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				var url = GetCurrentUrl();
				WebViewController.SendNavigated(
					new WebNavigatedEventArgs(_lastEvent, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
				);

				_renderer.UpdateCanGoBackForward();
			}

			[PortHandler("Partially ported")]
			public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (webView.IsLoading)
					return;

				var url = GetCurrentUrl();
				if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
					return;

				_renderer._ignoreSourceChanges = true;
				WebView.SetValueFromRenderer(WebView.SourceProperty, new UrlWebViewSource { Url = url });
				_renderer._ignoreSourceChanges = false;
				ProcessNavigated(url);
			}

			[PortHandler]
			async void ProcessNavigated(string url)
			{
				try
				{
					if (_renderer?.WebView?.Cookies != null)
						await _renderer.SyncNativeCookiesToElement(url);
				}
				catch (Exception exc)
				{
					Forms.MauiContext?.CreateLogger<WkWebViewRenderer>()?.LogWarning(exc, "Failed to Sync Cookies");
				}

				var args = new WebNavigatedEventArgs(_lastEvent, WebView.Source, url, WebNavigationResult.Success);
				WebViewController.SendNavigated(args);
				_renderer.UpdateCanGoBackForward();

			}

			public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
			}

			[PortHandler]
			// https://stackoverflow.com/questions/37509990/migrating-from-uiwebview-to-wkwebview
			public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
			{
				var navEvent = WebNavigationEvent.NewPage;
				var navigationType = navigationAction.NavigationType;
				switch (navigationType)
				{
					case WKNavigationType.LinkActivated:
						navEvent = WebNavigationEvent.NewPage;

						if (navigationAction.TargetFrame == null)
							webView?.LoadRequest(navigationAction.Request);

						break;
					case WKNavigationType.FormSubmitted:
						navEvent = WebNavigationEvent.NewPage;
						break;
					case WKNavigationType.BackForward:
						navEvent = _renderer._lastBackForwardEvent;
						break;
					case WKNavigationType.Reload:
						navEvent = WebNavigationEvent.Refresh;
						break;
					case WKNavigationType.FormResubmitted:
						navEvent = WebNavigationEvent.NewPage;
						break;
					case WKNavigationType.Other:
						navEvent = WebNavigationEvent.NewPage;
						break;
				}

				_lastEvent = navEvent;
				var request = navigationAction.Request;
				var lastUrl = request.Url.ToString();
				var args = new WebNavigatingEventArgs(navEvent, new UrlWebViewSource { Url = lastUrl }, lastUrl);

				WebViewController.SendNavigating(args);
				_renderer.UpdateCanGoBackForward();
				decisionHandler(args.Cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
			}

			[PortHandler]
			string GetCurrentUrl()
			{
				return _renderer?.Url?.AbsoluteUrl?.ToString();
			}
		}

		[PortHandler]
		class CustomWebViewUIDelegate : WKUIDelegate
		{
			static string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
			static string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");

			public override void RunJavaScriptAlertPanel(WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
			{
				PresentAlertController(
					webView,
					message,
					okAction: _ => completionHandler()
				);
			}

			public override void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
			{
				PresentAlertController(
					webView,
					message,
					okAction: _ => completionHandler(true),
					cancelAction: _ => completionHandler(false)
				);
			}

			public override void RunJavaScriptTextInputPanel(
				WKWebView webView, string prompt, string defaultText, WKFrameInfo frame, Action<string> completionHandler)
			{
				PresentAlertController(
					webView,
					prompt,
					defaultText: defaultText,
					okAction: x => completionHandler(x.TextFields[0].Text),
					cancelAction: _ => completionHandler(null)
				);
			}

			static string GetJsAlertTitle(WKWebView webView)
			{
				// Emulate the behavior of UIWebView dialogs.
				// The scheme and host are used unless local html content is what the webview is displaying,
				// in which case the bundle file name is used.

				if (webView.Url != null && webView.Url.AbsoluteString != $"file://{NSBundle.MainBundle.BundlePath}/")
					return $"{webView.Url.Scheme}://{webView.Url.Host}";

				return new NSString(NSBundle.MainBundle.BundlePath).LastPathComponent;
			}

			static UIAlertAction AddOkAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalOK, UIAlertActionStyle.Default, (_) => handler());
				controller.AddAction(action);
				controller.PreferredAction = action;
				return action;
			}

			static UIAlertAction AddCancelAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalCancel, UIAlertActionStyle.Cancel, (_) => handler());
				controller.AddAction(action);
				return action;
			}

			static void PresentAlertController(
				WKWebView webView,
				string message,
				string defaultText = null,
				Action<UIAlertController> okAction = null,
				Action<UIAlertController> cancelAction = null)
			{
				var controller = UIAlertController.Create(GetJsAlertTitle(webView), message, UIAlertControllerStyle.Alert);

				if (defaultText != null)
					controller.AddTextField((textField) => textField.Text = defaultText);

				if (okAction != null)
					AddOkAction(controller, () => okAction(controller));

				if (cancelAction != null)
					AddCancelAction(controller, () => cancelAction(controller));

				GetTopViewController(UIApplication.SharedApplication.GetKeyWindow().RootViewController)
					.PresentViewController(controller, true, null);
			}

			static UIViewController GetTopViewController(UIViewController viewController)
			{
				if (viewController is UINavigationController navigationController)
					return GetTopViewController(navigationController.VisibleViewController);

				if (viewController is UITabBarController tabBarController)
					return GetTopViewController(tabBarController.SelectedViewController);

				if (viewController.PresentedViewController != null)
					return GetTopViewController(viewController.PresentedViewController);

				return viewController;
			}
		}

		#region IPlatformRenderer implementation

		public UIView NativeView
		{
			get { return this; }
		}

		public UIViewController ViewController
		{
			get { return null; }
		}

		UIView ITabStop.TabStop => this;

		#endregion

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
		}
	}
}