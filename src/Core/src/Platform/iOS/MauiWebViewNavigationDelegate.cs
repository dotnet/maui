using System;
using Foundation;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewNavigationDelegate : WKNavigationDelegate
	{
		readonly WebViewHandler _handler;
		WebNavigationEvent _lastEvent;

		public MauiWebViewNavigationDelegate(WebViewHandler handler)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");
		}

		public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			if (_handler == null)
				return;

			if (webView.IsLoading)
				return;

			var url = GetCurrentUrl();

			if (url == $"file://{NSBundle.MainBundle.BundlePath}/")
				return;

			var virtualView = _handler.VirtualView;

			if (virtualView == null)
				return;

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Success);

			_handler.NativeView.UpdateCanGoBackForward(virtualView);
		}

		public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			if (_handler == null)
				return;

			var url = GetCurrentUrl();

			var virtualView = _handler.VirtualView;

			if (virtualView == null)
				return;

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Failure);

			_handler.NativeView?.UpdateCanGoBackForward(virtualView);
		}

		public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
		{
			if (_handler == null)
				return;

			var url = GetCurrentUrl();

			var virtualView = _handler.VirtualView;

			if (virtualView == null)
				return;

			virtualView.Navigated(_lastEvent, url, WebNavigationResult.Failure);

			_handler.NativeView?.UpdateCanGoBackForward(virtualView);

		}

		// https://stackoverflow.com/questions/37509990/migrating-from-uiwebview-to-wkwebview
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			if (_handler == null)
				return;

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
					navEvent = _handler.LastBackForwardWebNavigationEvent;
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

			var virtualView = _handler.VirtualView;

			if (virtualView == null)
				return;

			var request = navigationAction.Request;
			var lastUrl = request.Url.ToString();

			bool cancel = virtualView.Navigating(navEvent, lastUrl);
			_handler.NativeView?.UpdateCanGoBackForward(virtualView);
			decisionHandler(cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
		}

		string GetCurrentUrl()
		{
			return _handler.NativeView?.Url?.AbsoluteUrl?.ToString() ?? string.Empty;
		}
	}
}