#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public class WebViewProcessTerminatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebViewProcessTerminatedEventArgs"/> class.
		/// </summary>
		public WebViewProcessTerminatedEventArgs()
		{
		}

		internal WebViewProcessTerminatedEventArgs(PlatformWebViewProcessTerminatedEventArgs args = null)
		{
			PlatformArgs = args;
		}

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="WebViewProcessTerminatedEventArgs"/>.
		/// </summary>
		public PlatformWebViewProcessTerminatedEventArgs PlatformArgs { get; private set; }
	}
}