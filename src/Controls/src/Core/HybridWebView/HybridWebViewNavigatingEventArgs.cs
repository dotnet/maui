using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="HybridWebView.Navigating"/> event.
	/// </summary>
	public class HybridWebViewNavigatingEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="HybridWebViewNavigatingEventArgs"/>.
		/// </summary>
		/// <param name="url">The URI being navigated to.</param>
		/// <param name="target">The frame target of the navigation.</param>
		/// <param name="platformArgs">Platform-specific event arguments, or <c>null</c> if not available.</param>
		public HybridWebViewNavigatingEventArgs(Uri? url, WebNavigationTarget target, PlatformHybridWebViewNavigatingEventArgs? platformArgs)
		{
			Url = url;
			Target = target;
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets the URI being navigated to.
		/// </summary>
		public Uri? Url { get; }

		/// <summary>
		/// Gets the frame target of the navigation (main frame, iframe, or new window).
		/// </summary>
		public WebNavigationTarget Target { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the navigation should be cancelled.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets platform-specific event arguments that provide access to native navigation objects.
		/// </summary>
		public PlatformHybridWebViewNavigatingEventArgs? PlatformArgs { get; }
	}
}
