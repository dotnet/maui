using System;
#if IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
#elif ANDROID
using PlatformWebView = Android.Webkit.WebView;
#elif WINDOWS
using PlatformWebView = Microsoft.Web.WebView2.Core.CoreWebView2;
#endif

namespace Microsoft.Maui;

/// <summary>
/// Provides platform-specific information for the <see cref="IInitializationAwareWebView.WebViewInitializationCompleted(WebViewInitializationCompletedEventArgs)"/> event.
/// </summary>
public class WebViewInitializationCompletedEventArgs
{
#if IOS || MACCATALYST || ANDROID || WINDOWS

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	internal WebViewInitializationCompletedEventArgs(PlatformWebView? sender)
	{
		Sender = sender;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public PlatformWebView? Sender { get; }

#else

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	internal WebViewInitializationCompletedEventArgs()
	{
	}

#endif
}
