using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;
using WebKit;
using Xamarin.Forms.Internals;
using Uri = System.Uri;

namespace Xamarin.Forms.Platform.iOS
{
	public class WkWebViewRenderer : WKWebView, IVisualElementRenderer, IWebViewDelegate, IEffectControlProvider, ITabStop
	{
		EventTracker _events;
		bool _ignoreSourceChanges;
		WebNavigationEvent _lastBackForwardEvent;
		VisualElementPackager _packager;
#pragma warning disable 0414
		VisualElementTracker _tracker;
#pragma warning restore 0414
		public WkWebViewRenderer() : base(RectangleF.Empty, new WKWebViewConfiguration())
		{
		}

		public WkWebViewRenderer(WKWebViewConfiguration config) : base(RectangleF.Empty, config)
		{
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
			Element = element;
			Element.PropertyChanged += HandlePropertyChanged;
			WebView.EvalRequested += OnEvalRequested;
			WebView.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
			WebView.GoBackRequested += OnGoBackRequested;
			WebView.GoForwardRequested += OnGoForwardRequested;
			WebView.ReloadRequested += OnReloadRequested;
			NavigationDelegate = new CustomWebViewNavigationDelegate(this);
			UIDelegate = new CustomWebViewUIDelegate();

			BackgroundColor = UIColor.Clear;

			AutosizesSubviews = true;

			_tracker = new VisualElementTracker(this);

			_packager = new VisualElementPackager(this);
			_packager.Load();

			_events = new EventTracker(this);
			_events.LoadEvents(this);

			Load();

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				AccessibilityIdentifier = Element.AutomationId;

			if (element != null)
				element.SendViewInitialized(this);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public void LoadHtml(string html, string baseUrl)
		{
			if (html != null)
				LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		public void LoadUrl(string url)
		{
			var uri = new Uri(url);
			var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
			var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
			LoadRequest(new NSUrlRequest(new Uri(safeHostUri, safeRelativeUri)));
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// ensure that inner scrollview properly resizes when frame of webview updated
			ScrollView.Frame = Bounds;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (IsLoading)
					StopLoading();

				Element.PropertyChanged -= HandlePropertyChanged;
				WebView.EvalRequested -= OnEvalRequested;
				WebView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				WebView.GoBackRequested -= OnGoBackRequested;
				WebView.GoForwardRequested -= OnGoForwardRequested;
				WebView.ReloadRequested -= OnReloadRequested;

				_tracker?.Dispose();
				_packager?.Dispose();
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
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

			if (((WebView)Element).Source != null)
				((WebView)Element).Source.Load(this);

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

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			Reload();
		}

		void UpdateCanGoBackForward()
		{
			((IWebViewController)WebView).CanGoBack = CanGoBack;
			((IWebViewController)WebView).CanGoForward = CanGoForward;
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

			public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				var url = GetCurrentUrl();
				WebView.SendNavigated(
					new WebNavigatedEventArgs(_lastEvent, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
				);

				_renderer.UpdateCanGoBackForward();
			}

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

				var args = new WebNavigatedEventArgs(_lastEvent, WebView.Source, url, WebNavigationResult.Success);
				WebView.SendNavigated(args);

				_renderer.UpdateCanGoBackForward();
			}

			public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
			}

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

				WebView.SendNavigating(args);
				_renderer.UpdateCanGoBackForward();
				decisionHandler(args.Cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
			}

			string GetCurrentUrl()
			{
				return _renderer?.Url?.AbsoluteUrl?.ToString();
			}
		}

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

				GetTopViewController(UIApplication.SharedApplication.KeyWindow.RootViewController)
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