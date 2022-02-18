using System;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewNavigationDelegate : WKNavigationDelegate
	{
		readonly WebViewHandler _handler;

		public MauiWebViewNavigationDelegate(WebViewHandler handler)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");
		}

		public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			if (_handler != null)
				_handler.PlatformView.UpdateCanGoBackForward(_handler.VirtualView);

			base.DidFinishNavigation(webView, navigation);
		}
	}
}