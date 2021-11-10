using System;
using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;
		bool _firstRun = true;

		protected override AWebView CreateNativeView()
		{
			return new MauiWebView(Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
			};
		}

		public override void SetVirtualView(IView view)
		{
			_firstRun = true;
			base.SetVirtualView(view);
			// At this time all the mappers were already called
			_firstRun = false;
			ProcessSourceWhenReady(this, VirtualView);
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			ProcessSourceWhenReady(handler, webView);
		}

		public static void MapWebViewClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebViewClient(handler._webViewClient ??= new WebViewClient());
		}

		public static void MapWebChromeClient(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.SetWebChromeClient(handler._webChromeClient ??= new WebChromeClient());
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			string url = "javascript:" + script;

			if (url != null)
			{
				if (!url.StartsWith('/') && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
				{
					// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
					url = AssetBaseUrl + url;
				}

				handler.NativeView?.LoadUrl(url);
			}
		}
	
		public static void MapWebViewSettings(WebViewHandler handler, IWebView webView)
		{
			handler.NativeView.UpdateSettings(webView, true, true);
		}

		static void ProcessSourceWhenReady(WebViewHandler handler, IWebView webView)
		{
			//We want to load the source after making sure the mapper for webclients
			//and settings were called already
			if (handler._firstRun)
				return;

			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;
			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}
	}
}
