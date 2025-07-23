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
/// Provides platform-specific information about the <see cref="WebViewInitializingEventArgs"/> event.
/// </summary>
public class PlatformWebViewInitializingEventArgs
{
#if IOS || MACCATALYST || ANDROID || WINDOWS

	internal PlatformWebViewInitializingEventArgs(PlatformWebView? sender, PlatformSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	internal PlatformWebViewInitializingEventArgs(WebViewInitializationStartedEventArgs args)
		: this(args.Sender, args.Settings)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android, iOS, Mac Catalyst and Windows.
	/// </remarks>
	public PlatformWebView? Sender { get; }

	/// <summary>
	/// Gets the native settings attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android, iOS, Mac Catalyst and Windows.
	/// </remarks>
	public PlatformSettings Settings { get; }

#else

	internal PlatformWebViewInitializingEventArgs(WebViewInitializationStartedEventArgs args)
	{
	}

#endif
}
