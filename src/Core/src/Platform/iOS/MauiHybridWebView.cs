using System;
using Foundation;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class MauiHybridWebView : WKWebView, IHybridPlatformWebView
	{
		private readonly WeakReference<HybridWebViewHandler> _handler;

		public MauiHybridWebView(HybridWebViewHandler handler, RectangleF frame, WKWebViewConfiguration configuration)
			: base(frame, configuration)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);
		}

		public void SendRawMessage(string rawMessage)
		{
			EvaluateJavaScript(
				new NSString($"window.external.receiveMessage({System.Text.Json.JsonSerializer.Serialize(rawMessage)})"),
				(result, error) =>
				{
					// Handle the result or error here
				});
		}
	}
}
