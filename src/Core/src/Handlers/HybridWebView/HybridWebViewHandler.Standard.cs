using System;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapEvaluateJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg) { }

		public static void MapSendRawMessage(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg) { }
	}
}