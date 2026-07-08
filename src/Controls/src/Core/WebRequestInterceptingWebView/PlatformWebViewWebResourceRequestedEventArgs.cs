using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	internal PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
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

	static string? ToPlatformHeaders(IReadOnlyDictionary<string, string>? headers)
	{
		if (headers?.Count > 0)
		{
			var sb = new StringBuilder();
			foreach (var header in headers)
			{
				sb.AppendLine($"{header.Key}: {header.Value}");
			}
			return sb.ToString();
		}
		return null;
	}

	internal void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content)
	{
		// create the response
		RequestEventArgs.Response = Sender.Environment.CreateWebResourceResponse(
			content?.AsRandomAccessStream(),
			code,
			reason,
			ToPlatformHeaders(headers));
	}
	
	internal async Task SetResponseAsync(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask)
	{
		// Windows uses a deferral to let the webview know that we are going to be async
		using var deferral = RequestEventArgs.GetDeferral();

		// get the actual content
		var data = await contentTask;

		// create the response
		RequestEventArgs.Response = Sender.Environment.CreateWebResourceResponse(
			data?.AsRandomAccessStream(),
			code,
			reason,
			ToPlatformHeaders(headers));

		// let the webview know
		deferral.Complete();
	}

#elif IOS || MACCATALYST

	IReadOnlyDictionary<string, string>? _headers;

	internal PlatformWebViewWebResourceRequestedEventArgs(
		global::WebKit.WKWebView sender,
		global::WebKit.IWKUrlSchemeTask urlSchemeTask)
	{
		Sender = sender;
		UrlSchemeTask = urlSchemeTask;
	}

	internal PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
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

	static Foundation.NSMutableDictionary? ToPlatformHeaders(IReadOnlyDictionary<string, string>? headers)
	{
		if (headers?.Count > 0)
		{
			var dic = new Foundation.NSMutableDictionary();
			foreach (var header in headers)
			{
				dic.Add((Foundation.NSString)header.Key, (Foundation.NSString)header.Value);
			}
			return dic;
		}
		return null;
	}

	internal void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content)
	{
		// create and send the response headers
		UrlSchemeTask.DidReceiveResponse(new Foundation.NSHttpUrlResponse(
			Request.Url,
			code,
			"HTTP/1.1",
			ToPlatformHeaders(headers)));

		// send the data
		if (content is not null && Foundation.NSData.FromStream(content) is { } nsdata)
		{
			UrlSchemeTask.DidReceiveData(nsdata);
		}

		// let the webview know
		UrlSchemeTask.DidFinish();
	}

	internal async Task SetResponseAsync(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask)
	{
		// iOS and MacCatalyst will just wait until DidFinish is called

		// create and send the response headers
		UrlSchemeTask.DidReceiveResponse(new Foundation.NSHttpUrlResponse(
			Request.Url,
			code,
			"HTTP/1.1",
			ToPlatformHeaders(headers)));

		// get the actual content
		var data = await contentTask;

		// send the data
		if (data is not null && Foundation.NSData.FromStream(data) is { } nsdata)
		{
			UrlSchemeTask.DidReceiveData(nsdata);
		}

		// let the webview know
		UrlSchemeTask.DidFinish();
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

	internal PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
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

	static global::Android.Runtime.JavaDictionary<string, string>? ToPlatformHeaders(IReadOnlyDictionary<string, string>? headers, out string contentType)
	{
		contentType = "application/octet-stream";
		if (headers?.Count > 0)
		{
			var dic = new global::Android.Runtime.JavaDictionary<string, string>();
			foreach (var header in headers)
			{
				if ("Content-Type".Equals(header.Key, StringComparison.OrdinalIgnoreCase))
				{
					contentType = header.Value;
				}

				dic.Add(header.Key, header.Value);
			}
			return dic;
		}
		return null;
	}

	internal void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content)
	{
		// Android requires that we return immediately, even if the data is coming later

		// create and send the response headers
		var platformHeaders = ToPlatformHeaders(headers, out var contentType);
		Response = new global::Android.Webkit.WebResourceResponse(
			contentType,
			"UTF-8",
			code,
			reason,
			platformHeaders,
			content);
	}

	internal Task SetResponseAsync(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask)
	{
		// Android requires that we return immediately, even if the data is coming later

		// get the actual content
		var stream = new AsyncStream(contentTask, null);

		// create and send the response headers
		var platformHeaders = ToPlatformHeaders(headers, out var contentType);
		Response = new global::Android.Webkit.WebResourceResponse(
			contentType,
			"UTF-8",
			code,
			reason,
			platformHeaders,
			stream);

		return Task.CompletedTask;
	}

#else

	internal PlatformWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
	{
	}

#pragma warning disable CA1822 // Mark members as static
	internal string? GetRequestUri() => null;

	internal string? GetRequestMethod() => null;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => null;

	internal void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content) { }

	internal Task SetResponseAsync(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask) => Task.CompletedTask;
#pragma warning restore CA1822 // Mark members as static

#endif
}
