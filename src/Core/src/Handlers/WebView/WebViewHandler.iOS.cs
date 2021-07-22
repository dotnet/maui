using System;
using Foundation;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WKWebView>, IWebViewDelegate
	{
		static WKProcessPool? SharedPool;

		protected override WKWebView CreateNativeView()
		{
			return new WKWebView(RectangleF.Empty, CreateConfiguration())
			{
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true
			};
		}

		// https://developer.apple.com/forums/thread/99674
		// WKWebView and making sure cookies synchronize is really quirky
		// The main workaround I've found for ensuring that cookies synchronize 
		// is to share the Process Pool between all WkWebView instances.
		// It also has to be shared at the point you call init
		static WKWebViewConfiguration CreateConfiguration()
		{
			var config = new WKWebViewConfiguration();

			if (SharedPool == null)
				SharedPool = config.ProcessPool;
			else
				config.ProcessPool = SharedPool;
			
			return config;
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate webViewDelegate = handler;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}
				
		public void LoadHtml(string? html, string? baseUrl)
		{
			if (html != null)
				NativeView?.LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		public void LoadUrl(string? url)
		{
			var uri = new Uri(url ?? string.Empty);
			var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
			var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
			NSUrlRequest request = new NSUrlRequest(new Uri(safeHostUri, safeRelativeUri));

			NativeView?.LoadRequest(request);
		}
	}
}