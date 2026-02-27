#if WINDOWS
using Microsoft.Web.WebView2.Core;
#elif IOS || MACCATALYST
using WebKit;
#elif ANDROID
using Android.Webkit;
#endif

namespace Microsoft.Maui;

/// <summary>
/// Provides platform-specific information for the <see cref="IInitializationAwareWebView.WebViewInitializationCompleted(WebViewInitializationCompletedEventArgs)"/> event.
/// </summary>
public class WebViewInitializationCompletedEventArgs
{
#if IOS || MACCATALYST

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	/// <param name="configuration">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationCompletedEventArgs(WKWebView sender, WKWebViewConfiguration configuration)
	{
		Sender = sender;
		Configuration = configuration;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public WKWebView Sender { get; }

	/// <summary>
	/// Gets or sets the settings attached to the web view.
	/// </summary>
	public WKWebViewConfiguration Configuration { get; }

#elif ANDROID

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	/// <param name="settings">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationCompletedEventArgs(WebView sender, WebSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public WebView Sender { get; }

	/// <summary>
	/// Gets or sets the settings attached to the web view.
	/// </summary>
	public WebSettings Settings { get; }

#elif WINDOWS

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	/// <param name="settings">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal WebViewInitializationCompletedEventArgs(CoreWebView2 sender, CoreWebView2Settings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public CoreWebView2 Sender { get; }

	/// <summary>
	/// Gets or sets the settings attached to the web view.
	/// </summary>
	public CoreWebView2Settings Settings { get; }

#else

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	internal WebViewInitializationCompletedEventArgs()
	{
	}

#endif
}
