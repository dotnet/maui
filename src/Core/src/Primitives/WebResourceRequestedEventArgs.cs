namespace Microsoft.Maui;

/// <summary>
/// Provides platform-specific information for the <see cref="IWebRequestInterceptingWebView.WebResourceRequested"/> event.
/// </summary>
public class WebResourceRequestedEventArgs
{
#if WINDOWS

	internal WebResourceRequestedEventArgs(
		global::Microsoft.Web.WebView2.Core.CoreWebView2 sender,
		global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs eventArgs)
	{
		Sender = sender;
		RequestEventArgs = eventArgs;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Windows.
	/// </remarks>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2 Sender { get; }

	/// <summary>
	/// Gets the native event args attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Windows.
	/// </remarks>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs RequestEventArgs { get; }

#elif IOS || MACCATALYST

	internal WebResourceRequestedEventArgs(
		global::WebKit.WKWebView sender,
		global::WebKit.IWKUrlSchemeTask urlSchemeTask)
	{
		Sender = sender;
		UrlSchemeTask = urlSchemeTask;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on iOS and Mac Catalyst.
	/// </remarks>
	public global::WebKit.WKWebView Sender { get; }

	/// <summary>
	/// Gets the native event args attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on iOS and Mac Catalyst.
	/// </remarks>
	public global::WebKit.IWKUrlSchemeTask UrlSchemeTask { get; }

#elif ANDROID

	internal WebResourceRequestedEventArgs(
		global::Android.Webkit.WebView sender,
		global::Android.Webkit.IWebResourceRequest request)
	{
		Sender = sender;
		Request = request;
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android.
	/// </remarks>
	public global::Android.Webkit.WebView Sender { get; }

	/// <summary>
	/// Gets the native event args attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android.
	/// </remarks>
	public global::Android.Webkit.IWebResourceRequest Request { get; }

	/// <summary>
	/// Gets or sets the native response attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Android.
	/// </remarks>
	public global::Android.Webkit.WebResourceResponse? Response { get; set; }

#else

	internal WebResourceRequestedEventArgs()
	{
	}

#endif
}
