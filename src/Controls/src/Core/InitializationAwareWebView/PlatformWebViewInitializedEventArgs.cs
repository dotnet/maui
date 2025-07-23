#if IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
using PlatformSettings = WebKit.WKWebViewConfiguration;
#elif ANDROID
using PlatformWebView = Microsoft.Maui.Platform.MauiHybridWebView;
using PlatformSettings = Android.Webkit.WebSettings;
#elif WINDOWS
using PlatformWebView = Microsoft.Web.WebView2.Core.CoreWebView2;
using PlatformSettings = Microsoft.Web.WebView2.Core.CoreWebView2Settings;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides platform-specific information about the <see cref="WebViewInitializedEventArgs"/> event.
/// </summary>
public class PlatformWebViewInitializedEventArgs
{
#if IOS || MACCATALYST || ANDROID || WINDOWS

	internal PlatformWebViewInitializedEventArgs(PlatformWebView? sender)
	{
		Sender = sender;
	}

	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
		: this(args.Sender)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android, iOS, Mac Catalyst and Windows.
	/// </remarks>
	public PlatformWebView? Sender { get; }

#else

	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
	{
	}

#endif
}
