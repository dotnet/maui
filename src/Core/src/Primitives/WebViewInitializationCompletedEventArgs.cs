using System;
#if IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
#elif ANDROID
using PlatformWebView = Microsoft.Maui.Platform.MauiHybridWebView;
#elif WINDOWS
using PlatformWebView = Microsoft.Web.WebView2.Core.CoreWebView2;
#endif

namespace Microsoft.Maui;

public class WebViewInitializationCompletedEventArgs
{
#if IOS || MACCATALYST || ANDROID || WINDOWS

	internal WebViewInitializationCompletedEventArgs(PlatformWebView? sender)
	{
		Sender = sender;
	}

	public PlatformWebView? Sender { get; }

#else

	internal WebViewInitializationCompletedEventArgs()
	{
	}

#endif
}
