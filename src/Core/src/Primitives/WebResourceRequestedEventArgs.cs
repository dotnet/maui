namespace Microsoft.Maui;

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
	public global::Microsoft.Web.WebView2.Core.CoreWebView2 Sender { get; }

	/// <summary>
	/// 
	/// </summary>
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
	public global::WebKit.WKWebView Sender { get; }

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
	public global::Android.Webkit.WebView Sender { get; }

	/// <summary>
	/// 
	/// </summary>
	public global::Android.Webkit.IWebResourceRequest Request { get; }

	public global::Android.Webkit.WebResourceResponse? Response { get; set; }

#else

	internal WebResourceRequestedEventArgs()
	{
	}

#endif
}
