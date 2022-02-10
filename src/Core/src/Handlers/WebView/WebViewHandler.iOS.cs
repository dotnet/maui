using Microsoft.Maui.Graphics;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WKWebView>
	{
		protected virtual float MinimumSize => 44f;

		internal WebNavigationEvent _lastBackForwardEvent;

		protected override WKWebView CreateNativeView()
		{
			var nativeWebView = new MauiWKWebView(RectangleF.Empty)
			{
				NavigationDelegate = new MauiWebViewNavigationDelegate(this)
			};
			return nativeWebView;
		}

		internal WebNavigationEvent LastBackForwardWebNavigationEvent
		{
			get => _lastBackForwardEvent;
			set => _lastBackForwardEvent = value;
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.NativeView.CanGoBack)
				handler.LastBackForwardWebNavigationEvent = WebNavigationEvent.Back;

			handler.NativeView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.NativeView.CanGoForward)
				handler.LastBackForwardWebNavigationEvent = WebNavigationEvent.Forward;

			handler.NativeView?.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.NativeView?.UpdateReload(webView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.NativeView?.Eval(webView, script);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);

			var set = false;

			var width = widthConstraint;
			var height = heightConstraint;

			if (size.Width == 0)
			{
				if (widthConstraint <= 0 || double.IsInfinity(widthConstraint))
				{
					width = MinimumSize;
					set = true;
				}
			}

			if (size.Height == 0)
			{
				if (heightConstraint <= 0 || double.IsInfinity(heightConstraint))
				{
					height = MinimumSize;
					set = true;
				}
			}

			if (set)
				size = new Size(width, height);

			return size;
		}

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				if (handler.NativeView == null)
				{
					request.SetCanceled();
					return;
				}

				handler.NativeView.EvaluateJavaScript(request);
			}
		}
	}
}