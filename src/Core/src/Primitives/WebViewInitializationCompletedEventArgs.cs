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
	/// <param name="settings">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationCompletedEventArgs(PlatformWebView sender, PlatformSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public PlatformWebView Sender { get; }

	/// <summary>
	/// Gets or sets the settings for the web view.
	/// </summary>
	/// <remarks>
	/// This property can be used to configure various aspects of the web view, such as JavaScript support, caching, etc.
	/// </remarks>
	public PlatformSettings Settings { get; }

#else

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	internal WebViewInitializationCompletedEventArgs()
	{
	}

#endif
}
