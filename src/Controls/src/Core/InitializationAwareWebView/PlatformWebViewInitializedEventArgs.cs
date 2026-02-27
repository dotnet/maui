#if WINDOWS
using Microsoft.Web.WebView2.Core;
#elif IOS || MACCATALYST
using WebKit;
#elif ANDROID
using Android.Webkit;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides platform-specific information about the <see cref="WebViewInitializedEventArgs"/> event.
/// </summary>
public class PlatformWebViewInitializedEventArgs
{
#if IOS || MACCATALYST

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializationCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="sender">The native view that is being initialized.</param>
	/// <param name="configuration">The settings for the web view, which can be used to configure various aspects of the web view.</param>
	internal PlatformWebViewInitializedEventArgs(WKWebView sender, WKWebViewConfiguration configuration)
	{	
		Sender = sender;
		Configuration = configuration;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatformWebViewInitializedEventArgs"/> class.
	/// </summary>
	/// <param name="args">The event arguments containing the native view and configuration.</param>
	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
		: this(args.Sender, args.Configuration)
	{
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
	internal PlatformWebViewInitializedEventArgs(global::Android.Webkit.WebView sender, WebSettings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatformWebViewInitializedEventArgs"/> class.
	/// </summary>
	/// <param name="args">The event arguments containing the native view and configuration.</param>
	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
		: this(args.Sender, args.Settings)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::Android.Webkit.WebView Sender { get; }

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
	internal PlatformWebViewInitializedEventArgs(CoreWebView2 sender, CoreWebView2Settings settings)
	{
		Sender = sender;
		Settings = settings;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatformWebViewInitializedEventArgs"/> class.
	/// </summary>
	/// <param name="args">The event arguments containing the native view and configuration.</param>
	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
		: this(args.Sender, args.Settings)
	{
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

	internal PlatformWebViewInitializedEventArgs(WebViewInitializationCompletedEventArgs args)
	{
	}

#endif
}
