using System;
using Android.Graphics;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewClient : WebViewClient
	{
		WebViewHandler? _handler;

		public MauiWebViewClient(WebViewHandler handler)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");
		}

		public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
		{
			if (_handler?.VirtualView == null || string.IsNullOrWhiteSpace(url) || url == WebViewHandler.AssetBaseUrl)
				return;

			_handler.SyncPlatformCookiesToVirtualView(url);

			if (_handler != null)
				_handler.PlatformView.UpdateCanGoBackForward(_handler.VirtualView);

			base.OnPageStarted(view, url, favicon);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_handler = null;

			base.Dispose(disposing);
		}
	}
}