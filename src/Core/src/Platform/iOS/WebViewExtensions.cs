using System;
using System.Threading.Tasks;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this WKWebView platformWebView, IWebView webView)
		{
			platformWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this WKWebView platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				platformWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateUserAgent(this WKWebView platformWebView, IWebView webView)
		{
			if (webView.UserAgent != null)
				platformWebView.CustomUserAgent = webView.UserAgent;
			else
				webView.UserAgent =
					platformWebView.CustomUserAgent ??
					platformWebView.ValueForKey(new Foundation.NSString("userAgent"))?.ToString();
		}

		public static void UpdateGoBack(this WKWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoBack)
				platformWebView.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this WKWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoForward)
				platformWebView.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this WKWebView platformWebView, IWebView webView)
		{
			platformWebView?.Reload();
		}

		internal static void UpdateCanGoBackForward(this WKWebView platformWebView, IWebView webView)
		{
			webView.CanGoBack = platformWebView.CanGoBack;
			webView.CanGoForward = platformWebView.CanGoForward;
		}

		internal static void UpdateFlowDirectionForScrollView(this UIKit.UIScrollView scrollView, IView view)
		{
			scrollView.UpdateFlowDirection(view);

			// On macOS, we need to refresh the scroll indicators when flow direction changes
			// But only for runtime changes, not during initial load
			if (OperatingSystem.IsMacCatalyst() && view.IsLoadedOnPlatform())
			{
				bool showsVertical = scrollView.ShowsVerticalScrollIndicator;
				bool showsHorizontal = scrollView.ShowsHorizontalScrollIndicator;

				scrollView.ShowsVerticalScrollIndicator = false;
				scrollView.ShowsHorizontalScrollIndicator = false;

				scrollView.ShowsVerticalScrollIndicator = showsVertical;
				scrollView.ShowsHorizontalScrollIndicator = showsHorizontal;
			}
		}

		public static void Eval(this WKWebView platformWebView, IWebView webView, string script)
		{
			platformWebView.EvaluateJavaScriptAsync(script);
		}

		public static void EvaluateJavaScript(this WKWebView webView, EvaluateJavaScriptAsyncRequest request)
		{
			request.RunAndReport(EvaluateJavaScript(webView, request.Script));
		}

		static async Task<string> EvaluateJavaScript(WKWebView webView, string script)
		{
			var result = await webView.EvaluateJavaScriptAsync(script);
			return result?.ToString() ?? "null";
		}
	}
}