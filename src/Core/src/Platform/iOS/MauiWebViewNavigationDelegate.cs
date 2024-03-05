using System;
using Foundation;
using ObjCRuntime;
using WebKit;

namespace Microsoft.Maui.Platform
{
	// Use interface instead of base class here
	// https://developer.apple.com/documentation/webkit/wknavigationdelegate/1455641-webview?language=objc#discussion
	// The newer policy method is implemented in the base class
	// and the doc remarks state the older policy method is not called
	// if the newer one is implemented, but the new one is v13+
	// so we'd like to stick with the older one for now
	public class MauiWebViewNavigationDelegate : NSObject, IWKNavigationDelegate
	{
		readonly WeakReference<IWebViewHandler> _handler;
		WebNavigationEvent _lastEvent;

		public MauiWebViewNavigationDelegate(IWebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException("handler");
			_handler = new WeakReference<IWebViewHandler>(handler);
		}

		[Export("webView:didFinishNavigation:")]
		public void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;

			if (handler == null || virtualView == null)
				return;

			handler.PlatformView?.UpdateCanGoBackForward(handler.VirtualView);

			if (webView.IsLoading)
				return;

			var url = GetCurrentUrl();

			if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
				return;

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Success);

			// ProcessNavigatedAsync calls UpdateCanGoBackForward
			if (handler is WebViewHandler webViewHandler)
				webViewHandler.ProcessNavigatedAsync(url).FireAndForget();
			else
				handler.PlatformView?.UpdateCanGoBackForward(virtualView);
		}

		[Export("webView:didFailNavigation:withError:")]
		public void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;

			if (handler == null || virtualView == null)
				return;

			var url = GetCurrentUrl();

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Failure);

			handler.PlatformView?.UpdateCanGoBackForward(virtualView);
		}

		[Export("webView:didFailProvisionalNavigation:withError:")]
		public void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;

			if (handler == null || virtualView == null)
				return;

			var url = GetCurrentUrl();

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Failure);

			handler.PlatformView?.UpdateCanGoBackForward(virtualView);
		}

		// https://stackoverflow.com/questions/37509990/migrating-from-uiwebview-to-wkwebview
		[Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
		public void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;

			if (handler == null || virtualView == null)
			{
				decisionHandler.Invoke(WKNavigationActionPolicy.Cancel);
				return;
			}

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
					navEvent = CurrentNavigationEvent;
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

			bool cancel = virtualView.Navigating(navEvent, lastUrl);
			handler.PlatformView?.UpdateCanGoBackForward(virtualView);
			decisionHandler(cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
		}

		string GetCurrentUrl()
		{
			return Handler?.PlatformView?.Url?.AbsoluteUrl?.ToString() ?? string.Empty;
		}

		internal WebNavigationEvent CurrentNavigationEvent
		{
			get;
			set;
		}

		IWebViewHandler? Handler
		{
			get
			{
				if (_handler.TryGetTarget(out var handler))
				{
					return handler;
				}

				return null;
			}
		}
	}
}