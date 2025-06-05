using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Event arguments for the <see cref="HybridWebView.WebResourceRequested"/> event.
/// </summary>
public class HybridWebViewWebResourceRequestedEventArgs
{
	IReadOnlyDictionary<string, string>? _headers;
	IReadOnlyDictionary<string, string>? _queryParams;

	internal HybridWebViewWebResourceRequestedEventArgs(PlatformHybridWebViewWebResourceRequestedEventArgs platformArgs)
	{
		PlatformArgs = platformArgs;
		Uri = platformArgs.GetRequestUri() is string uri ? new Uri(uri) : throw new InvalidOperationException("Platform web request did not have a request URI.");
		Method = platformArgs.GetRequestMethod() ?? throw new InvalidOperationException("Platform web request did not have a request METHOD.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HybridWebViewWebResourceRequestedEventArgs"/> class
	/// with the specified URI and method.
	/// </summary>
	public HybridWebViewWebResourceRequestedEventArgs(Uri uri, string method)
	{
		Uri = uri;
		Method = method;
	}

	/// <summary>
	/// Gets the platform-specific event arguments.
	/// </summary>
	public PlatformHybridWebViewWebResourceRequestedEventArgs? PlatformArgs { get; }

	/// <summary>
	/// Gets the URI of the requested resource.
	/// </summary>
	public Uri Uri { get; }

	/// <summary>
	/// Gets the HTTP method used for the request (e.g., GET, POST).
	/// </summary>
	public string Method { get; }

	/// <summary>
	/// Gets the headers associated with the request.
	/// </summary>
	public IReadOnlyDictionary<string, string> Headers =>
		_headers ??= PlatformArgs?.GetRequestHeaders() ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Gets the query parameters from the URI.
	/// </summary>
	public IReadOnlyDictionary<string, string> QueryParameters =>
		_queryParams ??= WebUtils.ParseQueryString(Uri, false) ?? new Dictionary<string, string>(StringComparer.Ordinal);

	/// <summary>
	/// Gets or sets a value indicating whether the request has been handled.
	/// 
	/// If set to true, the web view will not process the request further and a response
	/// must be provided using the 
	/// <see cref="SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.IO.Stream?)"/> 
	/// or <see cref="SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.Threading.Tasks.Task{System.IO.Stream?})"/> method.
	/// If set to false, the web view will continue processing the request as normal.
	/// </summary>
	public bool Handled { get; set; }

	/// <summary>
	/// Sets the response for the web resource request. 
	/// 
	/// This method must be called if the <see cref="Handled"/> property is set to true.
	/// </summary>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="headers">The headers to include in the response.</param>
	/// <param name="content">The content of the response as a stream.</param>
	public void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content)
	{
		_ = PlatformArgs ?? throw new InvalidOperationException("Platform web request was not valid.");

#if WINDOWS

		// create the response
		PlatformArgs.RequestEventArgs.Response = PlatformArgs.Sender.Environment.CreateWebResourceResponse(
			content?.AsRandomAccessStream(),
			code,
			reason,
			PlatformHeaders(headers));

#elif IOS || MACCATALYST

		// iOS and MacCatalyst will just wait until DidFinish is called
		var task = PlatformArgs.UrlSchemeTask;

		// create and send the response headers
		task.DidReceiveResponse(new Foundation.NSHttpUrlResponse(
			PlatformArgs.Request.Url,
			code,
			"HTTP/1.1",
			PlatformHeaders(headers)));

		// send the data
		if (content is not null && Foundation.NSData.FromStream(content) is { } nsdata)
		{
			task.DidReceiveData(nsdata);
		}

		// let the webview know
		task.DidFinish();

#elif ANDROID

		// Android requires that we return immediately, even if the data is coming later

		// create and send the response headers
		var platformHeaders = PlatformHeaders(headers, out var contentType);
		PlatformArgs.Response = new global::Android.Webkit.WebResourceResponse(
			contentType,
			"UTF-8",
			code,
			reason,
			platformHeaders,
			content);

#endif
	}

	/// <summary>
	/// Sets the asynchronous response for the web resource request.
	/// 
	/// This method must be called if the <see cref="Handled"/> property is set to true.
	/// </summary>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="headers">The headers to include in the response.</param>
	/// <param name="contentTask">A task that represents the asynchronous operation of getting the response content.</param>
	/// <remarks>
	/// This method is not asynchronous and will return immediately. The actual response will be sent when the content task completes.
	/// </remarks>
	public void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask) =>
		SetResponseAsync(code, reason, headers, contentTask).FireAndForget();

#pragma warning disable CS1998 // Android implememntation does not use async/await
	async Task SetResponseAsync(int code, string reason, IReadOnlyDictionary<string, string>? headers, Task<Stream?> contentTask)
#pragma warning restore CS1998
	{
		_ = PlatformArgs ?? throw new InvalidOperationException("Platform web request was not valid.");

#if WINDOWS

		// Windows uses a deferral to let the webview know that we are going to be async
		using var deferral = PlatformArgs.RequestEventArgs.GetDeferral();

		// get the actual content
		var data = await contentTask;

		// create the response
		PlatformArgs.RequestEventArgs.Response = PlatformArgs.Sender.Environment.CreateWebResourceResponse(
			data?.AsRandomAccessStream(),
			code,
			reason,
			PlatformHeaders(headers));

		// let the webview know
		deferral.Complete();

#elif IOS || MACCATALYST

		// iOS and MacCatalyst will just wait until DidFinish is called
		var task = PlatformArgs.UrlSchemeTask;

		// create and send the response headers
		task.DidReceiveResponse(new Foundation.NSHttpUrlResponse(
			PlatformArgs.Request.Url,
			code,
			"HTTP/1.1",
			PlatformHeaders(headers)));

		// get the actual content
		var data = await contentTask;

		// send the data
		if (data is not null && Foundation.NSData.FromStream(data) is { } nsdata)
		{
			task.DidReceiveData(nsdata);
		}

		// let the webview know
		task.DidFinish();

#elif ANDROID

		// Android requires that we return immediately, even if the data is coming later

		// get the actual content
		var stream = new AsyncStream(contentTask, null);

		// create and send the response headers
		var platformHeaders = PlatformHeaders(headers, out var contentType);
		PlatformArgs.Response = new global::Android.Webkit.WebResourceResponse(
			contentType,
			"UTF-8",
			code,
			reason,
			platformHeaders,
			stream);

#endif
	}

#if WINDOWS
	static string? PlatformHeaders(IReadOnlyDictionary<string, string>? headers)
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
#elif IOS || MACCATALYST
	static Foundation.NSMutableDictionary? PlatformHeaders(IReadOnlyDictionary<string, string>? headers)
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
#elif ANDROID
	static global::Android.Runtime.JavaDictionary<string, string>? PlatformHeaders(IReadOnlyDictionary<string, string>? headers, out string contentType)
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
#endif
}

/// <summary>
/// Extension methods for the <see cref="HybridWebViewWebResourceRequestedEventArgs"/> class.
/// </summary>
public static class HybridWebViewWebResourceRequestedEventArgsExtensions
{
	/// <summary>
	/// Sets the response for the web resource request with a status code and reason.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason) =>
		e.SetResponse(code, reason, null, (Stream?)null);

	/// <summary>
	/// Sets the response for the web resource request with a status code, reason, and content type.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="contentType">The content type of the response.</param>
	/// <param name="content">The content of the response as a stream.</param>
	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason, string contentType, Stream? content) =>
		e.SetResponse(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, content);

	/// <summary>
	/// Sets the response for the web resource request with a status code, reason, and content type.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="contentType">The content type of the response.</param>
	/// <param name="contentTask">A task that represents the asynchronous operation of getting the response content.</param>
	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason, string contentType, Task<Stream?> contentTask) =>
		e.SetResponse(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, contentTask);
}
