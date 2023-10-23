using System;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, Gtk.Widget>
	{
		protected override Gtk.Widget CreatePlatformView() => new NotImplementedView();

		public static void MapSource(IWebViewHandler handler, IWebView webView) { }
		public static void MapGoBack(IWebViewHandler handler, IWebView webView, object? arg) { }
		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg) { }
		public static void MapReload(IWebViewHandler handler, IWebView webView, object? arg) { }
		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg) { }
		public static void MapEvaluateJavaScriptAsync(IWebViewHandler handler, IWebView webView, object? arg) { }
	}
}