using System;
#if IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
#elif ANDROID
using PlatformWebView = Microsoft.Maui.Platform.MauiHybridWebView;
#elif WINDOWS
using PlatformWebView = Microsoft.Web.WebView2;
#else
using PlatformWebView = System.Object;
#endif

namespace Microsoft.Maui;

public class WebViewInitializedEventArgs : EventArgs
{
	public PlatformWebView WebView { get; set; }

	public WebViewInitializedEventArgs(object webView)
	{
		if (webView is PlatformWebView platWebView)
		{
			WebView = platWebView;
		}
		else
		{
			throw new ArgumentException($"webView must be of type {nameof(PlatformWebView)}");
		}
	}
}
