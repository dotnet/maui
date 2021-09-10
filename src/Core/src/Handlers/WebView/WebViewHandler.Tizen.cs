using System;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, MauiWebView>
	{
		TWebView NativeWebView => NativeView.WebView;

		protected override MauiWebView CreateNativeView()
		{
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} should have been set by base class.");
			return new MauiWebView(NativeParent);
		}

		protected override void ConnectHandler(MauiWebView nativeView)
		{
			TChromium.Initialize();
			MauiApplication.Current.Terminated += (sender, arg) => TChromium.Shutdown();
		}

		protected override void DisconnectHandler(MauiWebView nativeView)
		{
			NativeWebView.StopLoading();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.NativeView as IWebViewDelegate;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}
	}
}