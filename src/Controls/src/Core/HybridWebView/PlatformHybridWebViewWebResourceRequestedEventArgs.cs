#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Controls;

public class PlatformHybridWebViewWebResourceRequestedEventArgs
{
#if WINDOWS
	internal PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::Microsoft.Web.WebView2.Core.CoreWebView2 sender,
		global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs eventArgs)
	{
		Sender = sender;
		RequestEventArgs = eventArgs;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.RequestEventArgs)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2 Sender { get; }

	/// <summary>
	/// 
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs RequestEventArgs { get; }

	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequest Request => RequestEventArgs.Request;

	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponse? Response
	{
		get => RequestEventArgs.Response;
		set => RequestEventArgs.Response = value;
	}

	internal string? GetRequestUri() => Request.Uri;

#elif IOS || MACCATALYST

	public PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::WebKit.WKWebView sender,
		global::WebKit.IWKUrlSchemeTask urlSchemeTask)
	{
		Sender = sender;
		UrlSchemeTask = urlSchemeTask;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.UrlSchemeTask)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::WebKit.WKWebView Sender { get; }

	public global::WebKit.IWKUrlSchemeTask UrlSchemeTask { get; }

	public global::Foundation.NSUrlRequest Request => UrlSchemeTask.Request;

	internal string? GetRequestUri() => Request.Url?.AbsoluteString;

#elif ANDROID

	public PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::Android.Webkit.WebView sender,
		global::Android.Webkit.IWebResourceRequest request)
	{
		Sender = sender;
		Request = request;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.Request)
	{
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

	internal string? GetRequestUri() => Request.Url?.ToString();

#else

	internal PlatformHybridWebViewWebResourceRequestedEventArgs()
	{
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
	{
	}

#pragma warning disable CA1822 // Mark members as static
	internal string? GetRequestUri() => null;
#pragma warning restore CA1822 // Mark members as static

#endif
}
