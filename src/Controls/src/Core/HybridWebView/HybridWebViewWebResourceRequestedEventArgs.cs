#pragma warning disable RS0016 // Add public types and members to the declared API
using System;

namespace Microsoft.Maui.Controls;

public class HybridWebViewWebResourceRequestedEventArgs
{
	internal HybridWebViewWebResourceRequestedEventArgs(PlatformHybridWebViewWebResourceRequestedEventArgs platformArgs)
	{
		PlatformArgs = platformArgs;
		RequestUri = platformArgs.GetRequestUri() is string uri ? new Uri(uri) : throw new InvalidOperationException("Platform web request did not have a request URI.");
	}

	public HybridWebViewWebResourceRequestedEventArgs(string uri)
		: this(new Uri(uri))
	{
	}

	public HybridWebViewWebResourceRequestedEventArgs(Uri uri)
	{
		RequestUri = uri;
	}

	public PlatformHybridWebViewWebResourceRequestedEventArgs? PlatformArgs { get; }

	public Uri RequestUri { get; }

	public bool Handled { get; set; }
}
