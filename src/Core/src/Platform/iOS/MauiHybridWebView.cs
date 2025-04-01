using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Foundation;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	// Note: This type is partial to allow for source generation to create a partial class for the nested RawMessageContext type
	public partial class MauiHybridWebView : WKWebView, IHybridPlatformWebView
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
				new NSString($"window.external.receiveMessage({JsonSerializer.Serialize(rawMessage, RawMessageContext.Default.String)})"),
				(result, error) =>
				{
					// Handle the result or error here
				});
		}

		[JsonSourceGenerationOptions()]
		[JsonSerializable(typeof(string))]
		internal partial class RawMessageContext : JsonSerializerContext
		{
		}
	}
}
