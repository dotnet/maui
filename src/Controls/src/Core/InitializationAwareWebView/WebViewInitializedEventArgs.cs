using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Event arguments for the <see cref="HybridWebView.WebViewInitialized"/> event.
/// </summary>
public class WebViewInitializedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializedEventArgs"/> class
	/// with the specified platform-specific arguments.
	/// </summary>
	/// <param name="platformArgs">The platform-specific event arguments.</param>
	public WebViewInitializedEventArgs(PlatformWebViewInitializedEventArgs platformArgs)
	{
		PlatformArgs = platformArgs;
	}

	/// <summary>
	/// Gets the platform-specific event arguments.
	/// </summary>
	public PlatformWebViewInitializedEventArgs? PlatformArgs { get; }
}
