using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Foundation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using UIKit;
using WebKit;
namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class IOSWebViewManager : WebViewManager
	{
		private const string AppOrigin = "app://0.0.0.0/";

		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly WKWebView _webview;

		public IOSWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, WKWebView webview, IServiceProvider services, Dispatcher dispatcher, IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, string hostPageRelativePath)
			: base(services, dispatcher, new Uri(AppOrigin), fileProvider, jsComponents, hostPageRelativePath)
		{
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler ?? throw new ArgumentNullException(nameof(blazorMauiWebViewHandler));
			_webview = webview ?? throw new ArgumentNullException(nameof(webview));

			InitializeWebView();
		}

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			using var nsUrl = new NSUrl(absoluteUri.ToString());
			using var request = new NSUrlRequest(nsUrl);
			_webview.LoadRequest(request);
		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers) =>
			TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
			_webview.EvaluateJavaScript(
				javascript: $"__dispatchMessageCallback(\"{messageJSStringLiteral}\")",
				completionHandler: (NSObject result, NSError error) => { });
		}

		internal void MessageReceivedInternal(Uri uri, string message)
		{
			MessageReceived(uri, message);
		}

		private void InitializeWebView()
		{
			_webview.NavigationDelegate = new WebViewNavigationDelegate(_blazorMauiWebViewHandler);
			_webview.UIDelegate = new WebViewUIDelegate(_blazorMauiWebViewHandler);
		}

		internal sealed class WebViewUIDelegate : WKUIDelegate
		{
            private static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
            private static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");
			private readonly BlazorWebViewHandler _webView;

			public WebViewUIDelegate(BlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
			}


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
				WKWebView webView, string prompt, string? defaultText, WKFrameInfo frame, Action<string> completionHandler)
			{
				PresentAlertController(
					webView,
					prompt,
					defaultText: defaultText,
					okAction: x => completionHandler(x.TextFields[0].Text!),
					cancelAction: _ => completionHandler(null!)
				);
			}

			private static string GetJsAlertTitle(WKWebView webView)
			{
				// Emulate the behavior of UIWebView dialogs.
				// The scheme and host are used unless local html content is what the webview is displaying,
				// in which case the bundle file name is used.
				if (webView.Url != null && webView.Url.AbsoluteString != $"file://{NSBundle.MainBundle.BundlePath}/")
					return $"{webView.Url.Scheme}://{webView.Url.Host}";

				return new NSString(NSBundle.MainBundle.BundlePath).LastPathComponent;
			}

			private static UIAlertAction AddOkAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalOK, UIAlertActionStyle.Default, (_) => handler());
				controller.AddAction(action);
				controller.PreferredAction = action;
				return action;
			}

			private static UIAlertAction AddCancelAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalCancel, UIAlertActionStyle.Cancel, (_) => handler());
				controller.AddAction(action);
				return action;
			}

			private static void PresentAlertController(
				WKWebView webView,
				string message,
				string? defaultText = null,
				Action<UIAlertController>? okAction = null,
				Action<UIAlertController>? cancelAction = null)
			{
				var controller = UIAlertController.Create(GetJsAlertTitle(webView), message, UIAlertControllerStyle.Alert);

				if (defaultText != null)
					controller.AddTextField((textField) => textField.Text = defaultText);

				if (okAction != null)
					AddOkAction(controller, () => okAction(controller));

				if (cancelAction != null)
					AddCancelAction(controller, () => cancelAction(controller));

				GetTopViewController(UIApplication.SharedApplication.Windows.FirstOrDefault(m => m.IsKeyWindow)?.RootViewController)?
					.PresentViewController(controller, true, null);
			}

			private static UIViewController? GetTopViewController(UIViewController? viewController)
			{
				if (viewController is UINavigationController navigationController)
					return GetTopViewController(navigationController.VisibleViewController);

				if (viewController is UITabBarController tabBarController)
					return GetTopViewController(tabBarController.SelectedViewController!);

				if (viewController?.PresentedViewController != null)
					return GetTopViewController(viewController.PresentedViewController);

				return viewController;
			}
		}

		internal class WebViewNavigationDelegate : WKNavigationDelegate
		{
			private readonly BlazorWebViewHandler _webView;

			private WKNavigation? _currentNavigation;
			private Uri? _currentUri;

			public WebViewNavigationDelegate(BlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
			}

			public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
				_currentNavigation = navigation;
			}

			public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
			{
				if (navigationAction.TargetFrame!.MainFrame)
				{
					_currentUri = navigationAction.Request.Url;
				}
				decisionHandler(WKNavigationActionPolicy.Allow);
			}

			public override void DidReceiveServerRedirectForProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
				// We need to intercept the redirects to the app scheme because Safari will block them.
				// We will handle these redirects through the Navigation Manager.
				if (_currentUri?.Host == "0.0.0.0")
				{
					var uri = _currentUri;
					_currentUri = null;
					_currentNavigation = null;
					var request = new NSUrlRequest(uri);
					webView.LoadRequest(request);
				}
			}

			public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationStarting(_currentUri);
				}
			}

			public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationFinished(_currentUri);
					_currentUri = null;
					_currentNavigation = null;
				}
			}
		}
	}
}
