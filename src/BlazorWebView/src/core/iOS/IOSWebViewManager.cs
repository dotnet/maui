using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Foundation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class IOSWebViewManager : WebViewManager
	{
		internal void MessageReceivedInternal(Uri uri, string message)
		{
			MessageReceived(uri, message);
		}

		private const string AppOrigin = "app://0.0.0.0/";

		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly WKWebView _webview;

		public IOSWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, WKWebView webview, IServiceProvider services, Dispatcher dispatcher, IFileProvider fileProvider, string hostPageRelativePath)
			: base(services, dispatcher, new Uri(AppOrigin), fileProvider, hostPageRelativePath)
		{
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler ?? throw new ArgumentNullException(nameof(blazorMauiWebViewHandler));
			_webview = webview ?? throw new ArgumentNullException(nameof(webview));

			InitializeWebView();
		}

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			System.Console.WriteLine($"1111111111-NavigateCore - {absoluteUri}");
			using var nsUrl = new NSUrl(absoluteUri.ToString());
			using var request = new NSUrlRequest(nsUrl);
			_webview.LoadRequest(request);
		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out string headers) =>
			TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			System.Console.WriteLine($"1111111111-SendMessage {message}");
			var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
			_webview.EvaluateJavaScript(
				javascript: $"__dispatchMessageCallback(\"{messageJSStringLiteral}\")",
				completionHandler: (NSObject result, NSError error) => { });
		}

		private void InitializeWebView()
		{
			System.Console.WriteLine($"1111111111-InitializeWebView start");

			_webview.NavigationDelegate = new WebViewNavigationDelegate(_blazorMauiWebViewHandler);

			System.Console.WriteLine($"1111111111-InitializeWebView end");
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
					System.Console.WriteLine($"1111111111-DidCommitNavigation");
					//_webView.HandleNavigationStarting(_currentUri);
				}
			}

			public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					System.Console.WriteLine($"1111111111-DidFinishNavigation");
					//_webView.HandleNavigationFinished(_currentUri);
					_currentUri = null;
					_currentNavigation = null;
				}
			}
		}
	}
}
