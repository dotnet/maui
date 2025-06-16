using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides platform-specific information about the <see cref="WebViewWebResourceRequestedEventArgs"/> event.
/// </summary>
public class PlatformWebViewWebResourceRequestedEventArgs
{
#if WINDOWS

	IReadOnlyDictionary<string, string>? _headers;

	internal PlatformWebViewWebResourceRequestedEventArgs(
		global::Microsoft.Web.WebView2.Core.CoreWebView2 sender,
		global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs eventArgs)
	{
		Sender = sender;
		RequestEventArgs = eventArgs;
	}

	public PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.RequestEventArgs)
	{
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

	/// <summary>
	/// Gets the native request attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on Windows.
	/// This is equivalent to RequestEventArgs.Request.
	/// </remarks>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequest Request => RequestEventArgs.Request;

	internal string? GetRequestUri() => Request.Uri;

	internal string? GetRequestMethod() => Request.Method;

	internal IReadOnlyDictionary<string, string> GetRequestHeaders() =>
		_headers ??= new Dictionary<string, string>(Request.Headers, StringComparer.OrdinalIgnoreCase);

#elif IOS || MACCATALYST

	IReadOnlyDictionary<string, string>? _headers;

	internal PlatformWebViewWebResourceRequestedEventArgs(
		global::WebKit.WKWebView sender,
		global::WebKit.IWKUrlSchemeTask urlSchemeTask)
	{
		Sender = sender;
		UrlSchemeTask = urlSchemeTask;
	}

	public PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.UrlSchemeTask)
	{
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

	/// <summary>
	/// Gets the native request attached to the event.
	/// </summary>
	/// <remarks>
	/// This is only available on iOS and Mac Catalyst.
	/// This is equivalent to UrlSchemeTask.Request.
	/// </remarks>
	public Foundation.NSUrlRequest Request => UrlSchemeTask.Request;

	internal string? GetRequestUri() => Request.Url?.AbsoluteString;

	internal string? GetRequestMethod() => Request.HttpMethod;

	internal IReadOnlyDictionary<string, string> GetRequestHeaders() =>
		_headers ??= CreateHeadersDictionary();

	Dictionary<string, string> CreateHeadersDictionary()
	{
		var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (Request.Headers is { } rh)
		{
			foreach (var key in rh.Keys)
			{
				if (key is Foundation.NSString keyString)
				{
					headers[keyString] = rh[keyString].ToString();
				}
			}
		}
		return headers;
	}

#elif ANDROID

	Action<global::Android.Webkit.WebResourceResponse?> _setResponse;
	global::Android.Webkit.WebResourceResponse? _response;
	IReadOnlyDictionary<string, string>? _headers;

	internal PlatformWebViewWebResourceRequestedEventArgs(
		global::Android.Webkit.WebView sender,
		global::Android.Webkit.IWebResourceRequest request,
		Action<global::Android.Webkit.WebResourceResponse?> setResponse)
	{
		Sender = sender;
		Request = request;
		_setResponse = setResponse;
	}

	public PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.Request, (response) => args.Response = response)
	{
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
	/// Gets or sets the native response to return to the web view.
	///
	/// This property must be set to a valid response if the <see cref="WebViewWebResourceRequestedEventArgs.Handled"/> property is set to true.
	///
	/// This is only available on Android.
	/// </summary>
	public global::Android.Webkit.WebResourceResponse? Response
	{
		get => _response;
		set
		{
			_response = value;
			_setResponse(value);
		}
	}

	internal string? GetRequestUri() => Request.Url?.ToString();

	internal string? GetRequestMethod() => Request.Method;

	internal IReadOnlyDictionary<string, string> GetRequestHeaders() =>
		_headers ??= Request.RequestHeaders is { } rh
			? new Dictionary<string, string>(rh, StringComparer.OrdinalIgnoreCase)
			: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

#else

	public PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
	{
	}

#pragma warning disable CA1822 // Mark members as static
	internal string? GetRequestUri() => null;

	internal string? GetRequestMethod() => null;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => null;
#pragma warning restore CA1822 // Mark members as static

#endif
}
