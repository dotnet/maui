using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Event arguments for the <see cref="HybridWebView.WebResourceRequested"/> event.
/// </summary>
public class WebViewWebResourceRequestedEventArgs : EventArgs
{
	IReadOnlyDictionary<string, string>? _headers;
	IReadOnlyDictionary<string, string>? _queryParams;

	public WebViewWebResourceRequestedEventArgs(PlatformWebViewWebResourceRequestedEventArgs platformArgs)
	{
		PlatformArgs = platformArgs;
		Uri = platformArgs.GetRequestUri() is string uri ? new Uri(uri) : throw new InvalidOperationException("Platform web request did not have a request URI.");
		Method = platformArgs.GetRequestMethod() ?? throw new InvalidOperationException("Platform web request did not have a request METHOD.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewWebResourceRequestedEventArgs"/> class
	/// with the specified URI and method.
	/// </summary>
	public WebViewWebResourceRequestedEventArgs(Uri uri, string method)
	{
		Uri = uri;
		Method = method;
	}

	/// <summary>
	/// Gets the platform-specific event arguments.
	/// </summary>
	public PlatformWebViewWebResourceRequestedEventArgs? PlatformArgs { get; }

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
	public void SetResponse(int code, string reason, IReadOnlyDictionary<string, string>? headers, Stream? content) =>
		PlatformArgs?.SetResponse(code, reason, headers, content);

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
		PlatformArgs?.SetResponseAsync(code, reason, headers, contentTask).FireAndForget();

	/// <summary>
	/// Sets the response for the web resource request with a status code and reason.
	/// </summary>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	public void SetResponse(int code, string reason) =>
		PlatformArgs?.SetResponse(code, reason, null, null);

	/// <summary>
	/// Sets the response for the web resource request with a status code, reason, and content type.
	/// </summary>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="contentType">The content type of the response.</param>
	/// <param name="content">The content of the response as a stream.</param>
	public void SetResponse(int code, string reason, string contentType, Stream? content) =>
		PlatformArgs?.SetResponse(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, content);

	/// <summary>
	/// Sets the response for the web resource request with a status code, reason, and content type.
	/// </summary>
	/// <param name="code">The HTTP status code for the response.</param>
	/// <param name="reason">The reason phrase for the response.</param>
	/// <param name="contentType">The content type of the response.</param>
	/// <param name="contentTask">A task that represents the asynchronous operation of getting the response content.</param>
	public void SetResponse(int code, string reason, string contentType, Task<Stream?> contentTask) =>
		PlatformArgs?.SetResponseAsync(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, contentTask).FireAndForget();
}
