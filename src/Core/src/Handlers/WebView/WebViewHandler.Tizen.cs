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
			platformView.NavigationPolicyDecided += OnNavigationPolicyDecided;
		}

		protected override void DisconnectHandler(MauiWebView platformView)
		{
			if (!platformView.HasBody())
				return;

			base.DisconnectHandler(platformView);
			platformView.PageLoadFinished -= OnPageLoadFinished;
			platformView.NavigationPolicyDecided -= OnNavigationPolicyDecided;
		}

		// Enforces AllowedDomains at the navigation level on Tizen. When no allowlist is configured,
		// IsUrlAllowed returns true and the navigation proceeds (Use), preserving default behavior.
		void OnNavigationPolicyDecided(object? sender, Tizen.NUI.WebViewPolicyDecidedEventArgs e)
		{
			var maker = e.ResponsePolicyDecisionMaker;
			if (maker is null)
				return;

			if (WebViewDomainAllowlist.IsUrlAllowed(maker.Url, VirtualView))
				maker.Use();
			else
				maker.Ignore();
		}

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView?.UpdateSource(webView, handler.PlatformView);
		}

		public static void MapUserAgent(IWebViewHandler handler, IWebView webView)
		{
			handler.PlatformView?.UpdateUserAgent(webView);
		}

		public static void MapGoBack(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView.UpdateReload(webView);
		}

		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.EvaluateJavaScript(script);
		}

		public static void MapEvaluateJavaScriptAsync(IWebViewHandler handler, IWebView webView, object? arg)
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
