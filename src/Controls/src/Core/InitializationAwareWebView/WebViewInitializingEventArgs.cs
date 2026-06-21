using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Event arguments for the <see cref="HybridWebView.WebViewInitializing"/> event.
/// </summary>
public class WebViewInitializingEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WebViewInitializingEventArgs"/> class
	/// with the specified platform-specific arguments.
	/// </summary>
	/// <param name="platformArgs">The platform-specific event arguments.</param>
	public WebViewInitializingEventArgs(PlatformWebViewInitializingEventArgs platformArgs)
	{
		PlatformArgs = platformArgs;
	}

	/// <summary>
	/// Gets the platform-specific event arguments.
	/// </summary>
	public PlatformWebViewInitializingEventArgs? PlatformArgs { get; }
}
