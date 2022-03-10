namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, MauiWebView>
	{
		protected virtual double MinimumSize => 44d;

		protected override MauiWebView CreatePlatformView() => new()
		{
			MinimumSize = new Tizen.NUI.Size2D(MinimumSize.ToScaledPixel(), MinimumSize.ToScaledPixel()),
		};

		protected override void ConnectHandler(MauiWebView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.PageLoadFinished += OnPageLoadFinished;
		}

		protected override void DisconnectHandler(MauiWebView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.PageLoadFinished -= OnPageLoadFinished;
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			handler.PlatformView?.UpdateSource(webView, handler.PlatformView);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView.UpdateReload(webView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.EvaluateJavaScript(script);
		}

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				handler.PlatformView.EvaluateJavaScript(request);
			}
		}

		void OnPageLoadFinished(object? sender, Tizen.NUI.WebViewPageLoadEventArgs e)
		{
			PlatformView?.UpdateCanGoBackForward(VirtualView);
		}

	}
}
