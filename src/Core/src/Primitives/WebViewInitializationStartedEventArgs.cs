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

namespace Microsoft.Maui;

/// <summary>
/// Provides platform-specific information for the <see cref="IInitializationAwareWebView.WebViewInitializationStarted(WebViewInitializationStartedEventArgs)"/> event.
/// </summary>
public class WebViewInitializationStartedEventArgs
{
#if IOS || MACCATALYST || ANDROID || WINDOWS

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationStartedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	/// <param name="settings">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationStartedEventArgs(PlatformWebView sender, PlatformSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationStartedEventArgs"/> class.
	/// </summary>
	/// <param name="settings">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationStartedEventArgs(PlatformSettings settings)
	{
		Sender = null;
		Settings = settings;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This property is not available on all platforms.
	/// </remarks>
	public PlatformWebView? Sender { get; }

	/// <summary>
	/// Gets or sets the settings for the web view.
	/// </summary>
	/// <remarks>
	/// This property can be used to configure various aspects of the web view, such as JavaScript support, caching, etc.
	/// </remarks>
	public PlatformSettings Settings { get; }

#else

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationStartedEventArgs"/> class.
	/// </summary>
	/// <remarks>
	/// This constructor is used when the platform does not support web view initialization events.
	/// </remarks>
	internal WebViewInitializationStartedEventArgs()
	{
	}

#endif
}
