#if IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
using PlatformSettings = WebKit.WKWebViewConfiguration;
#elif ANDROID
using PlatformWebView = Android.Webkit.WebView;
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

	internal PlatformWebViewInitializedEventArgs(PlatformWebView sender, PlatformSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
		: this(args.Sender, args.Settings)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android, iOS, Mac Catalyst and Windows.
	/// </remarks>
	public PlatformWebView Sender { get; }

	/// <summary>
	/// Gets or sets the settings for the web view.
	/// </summary>
	/// <remarks>
	/// This property can be used to configure various aspects of the web view, such as JavaScript support, caching, etc.
	/// </remarks>
	public PlatformSettings Settings { get; }

#else

	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
	{
	}

#endif
}
