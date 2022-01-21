using Microsoft.Maui.Graphics;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WKWebView>
	{
		protected virtual float MinimumSize => 44f;

		protected override WKWebView CreateNativeView() => new MauiWKWebView(RectangleF.Empty);

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack)
				nativeWebView.GoBack();
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			var nativeWebView = handler.NativeView;

			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward)
				nativeWebView.GoForward();
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			// TODO: Sync Cookies

			handler.NativeView?.Reload();
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
	}
}