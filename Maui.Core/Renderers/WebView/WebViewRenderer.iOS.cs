using System.Maui.Core.Platform;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace System.Maui.Platform
{
	public partial class WebViewRenderer : AbstractViewRenderer<IWebView, WKWebView>, IWebViewDelegate
	{
		protected bool _ignoreSourceChanges;
		protected WebNavigationEvent _lastBackForwardEvent;

		protected override WKWebView CreateView()
		{
			var wKWebView = new WKWebView(RectangleF.Empty, new WKWebViewConfiguration())
			{
				NavigationDelegate = new CustomWebViewNavigationDelegate(this),
				UIDelegate = new CustomWebViewUIDelegate()
			};

			VirtualView.EvalRequested += OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested += OnGoBackRequested;
			VirtualView.GoForwardRequested += OnGoForwardRequested;
			VirtualView.ReloadRequested += OnReloadRequested;

			return wKWebView;
		}

		protected override void DisposeView(WKWebView wKWebView)
		{
			VirtualView.EvalRequested -= OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested -= OnGoBackRequested;
			VirtualView.GoForwardRequested -= OnGoForwardRequested;
			VirtualView.ReloadRequested -= OnReloadRequested;

			base.DisposeView(wKWebView);
		}

		public static void MapPropertySource(IViewRenderer renderer, IWebView webView)
		{
			(renderer as WebViewRenderer)?.Load();
		}

		public void LoadHtml(string html, string baseUrl)
		{
			if (html != null)
				TypedNativeView.LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		public void LoadUrl(string url)
		{
			var uri = new Uri(url);
			var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
			var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
			NSUrlRequest request = new NSUrlRequest(new Uri(safeHostUri, safeRelativeUri));

			TypedNativeView.LoadRequest(request);
		}

		protected internal void UpdateCanGoBackForward()
		{
			VirtualView.CanGoBack = TypedNativeView.CanGoBack;
			VirtualView.CanGoForward = TypedNativeView.CanGoForward;
		}

		void Load()
		{
			if (_ignoreSourceChanges)
				return;

			if (VirtualView.Source != null)
				VirtualView.Source.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			TypedNativeView.EvaluateJavaScriptAsync(eventArg.Script);
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var result = await TypedNativeView.EvaluateJavaScriptAsync(script);
			return result?.ToString();
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoBack)
			{
				_lastBackForwardEvent = WebNavigationEvent.Back;
				TypedNativeView.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoForward)
			{
				_lastBackForwardEvent = WebNavigationEvent.Forward;
				TypedNativeView.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			TypedNativeView.Reload();
		}

		class CustomWebViewNavigationDelegate : WKNavigationDelegate
		{
			readonly WebViewRenderer _webViewRenderer;
			WebNavigationEvent _lastEvent;

			public CustomWebViewNavigationDelegate(WebViewRenderer webViewRenderer)
			{
				_webViewRenderer = webViewRenderer ?? throw new ArgumentNullException("renderer");
			}

			IWebView WebView => _webViewRenderer.VirtualView;

			public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				var url = GetCurrentUrl();
				WebView.Navigated(
					new WebNavigatedEventArgs(_lastEvent, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Failure)
				);

				_webViewRenderer.UpdateCanGoBackForward();
			}

			public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (webView.IsLoading)
					return;

				var url = GetCurrentUrl();
				if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
					return;

				_webViewRenderer._ignoreSourceChanges = true;
				WebView.Source = new UrlWebViewSource { Url = url };
				_webViewRenderer._ignoreSourceChanges = false;

				var args = new WebNavigatedEventArgs(_lastEvent, WebView.Source, url, WebNavigationResult.Success);
				WebView.Navigated(args);

				_webViewRenderer.UpdateCanGoBackForward();

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
						navEvent = _webViewRenderer._lastBackForwardEvent;
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

				WebView.Navigating(args);
				_webViewRenderer.UpdateCanGoBackForward();
				decisionHandler(args.Cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
			}

			string GetCurrentUrl()
			{
				return _webViewRenderer?.TypedNativeView?.Url?.AbsoluteUrl?.ToString();
			}
		}

		class CustomWebViewUIDelegate : WKUIDelegate
		{
			static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
			static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");

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
	}
}