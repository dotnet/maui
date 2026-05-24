#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base event arguments for <see cref="WebView"/> navigation events.
	/// </summary>
	public class WebNavigationEventArgs : EventArgs
	{
		protected WebNavigationEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
		{
			NavigationEvent = navigationEvent;
			Source = source;
			Url = url;
		}

		/// <summary>
		/// Gets the type of navigation event that occurred.
		/// </summary>
		public WebNavigationEvent NavigationEvent { get; internal set; }

		/// <summary>
		/// Gets the source of the web view content.
		/// </summary>
		public WebViewSource Source { get; internal set; }

		/// <summary>
		/// Gets the URL associated with the navigation event.
		/// </summary>
		public string Url { get; internal set; }
	}
}