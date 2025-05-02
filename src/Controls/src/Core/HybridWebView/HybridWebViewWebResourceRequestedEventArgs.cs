using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls;

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

	public HybridWebViewWebResourceRequestedEventArgs(Uri uri, string method)
	{
		Uri = uri;
		Method = method;
	}

	public PlatformHybridWebViewWebResourceRequestedEventArgs? PlatformArgs { get; }

	public Uri Uri { get; }

	public string Method { get; }

	public IReadOnlyDictionary<string, string> Headers =>
		_headers ??= PlatformArgs?.GetRequestHeaders() ?? new Dictionary<string, string>();

	public IReadOnlyDictionary<string, string> QueryParameters =>
		_queryParams ??= WebUtils.ParseQueryString(Uri, false) ?? new Dictionary<string, string>();

	public bool Handled { get; set; }

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

public static class HybridWebViewWebResourceRequestedEventArgsExtensions
{
	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason) =>
		e.SetResponse(code, reason, null, (Stream?)null);

	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason, string contentType, Stream? content) =>
		e.SetResponse(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, content);

	public static void SetResponse(this HybridWebViewWebResourceRequestedEventArgs e, int code, string reason, string contentType, Task<Stream?> contentTask) =>
		e.SetResponse(code, reason, new Dictionary<string, string> { ["Content-Type"] = contentType }, contentTask);
}
