using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapSource(IViewHandler handler, IWebView webView) { }

		public static void MapGoBack(IViewHandler handler, IWebView webView, object? arg) { }
		public static void MapGoForward(IViewHandler handler, IWebView webView, object? arg) { }
		public static void MapReload(IViewHandler handler, IWebView webView, object? arg) { }
	}
}