#pragma warning disable RS0016 // Add public types and members to the declared API
using System;
using System.Collections.Generic;

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
}
