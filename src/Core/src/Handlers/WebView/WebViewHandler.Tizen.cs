namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, MauiWebView>
	{
		protected virtual double MinimumSize => 44d;

		protected override MauiWebView CreatePlatformView() => new()
		{
			MinimumSize = new Tizen.NUI.Size2D(MinimumSize.ToScaledPixel(), MinimumSize.ToScaledPixel()),
		};

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView?.UpdateSource(webView, handler.PlatformView);
		}

		[MissingMapper]
		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
		}

		[MissingMapper]
		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
		}

		[MissingMapper]
		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
		}

		[MissingMapper]
		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
		}
		
		[MissingMapper]
		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
		}

	}
}
