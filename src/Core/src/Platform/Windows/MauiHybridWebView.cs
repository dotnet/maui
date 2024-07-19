using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Platform
{
	public class MauiHybridWebView : WebView2, IHybridPlatformWebView
	{
		private readonly WeakReference<HybridWebViewHandler> _handler;

		public MauiHybridWebView(HybridWebViewHandler handler)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);
		}

		public void SendRawMessage(string rawMessage)
		{
			CoreWebView2.PostWebMessageAsString(rawMessage);
		}
	}
}
