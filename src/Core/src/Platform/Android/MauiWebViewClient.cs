using System;
using Android.Graphics;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewClient : WebViewClient
	{
		readonly WebViewHandler _handler;

		public MauiWebViewClient(WebViewHandler handler)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");
		}

		public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
		{
			if (_handler != null)
				_handler.PlatformView.UpdateCanGoBackForward(_handler.VirtualView);

			base.OnPageStarted(view, url, favicon);
		}
	}
}