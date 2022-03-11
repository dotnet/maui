using System;

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Used to provide information about a link (<![CDATA[<a>]]>) clicked within a Blazor WebView.
	/// <para>
	/// Anchor tags with target="_blank" will always open in the default
	/// browser and the ExternalNavigationStarting event won't be called.
	/// </para>
	/// </summary>
	public class ExternalLinkNavigationEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ExternalLinkNavigationEventArgs" />.
		/// </summary>
		/// <param name="uri">The external <see cref="Uri">URI</see> to be navigated to.</param>
		public ExternalLinkNavigationEventArgs(Uri uri)
		{
			Uri = uri;
		}

		/// <summary>
		/// Gets the external <see cref="Uri">URI</see> to be navigated to.
		/// </summary>
		public Uri Uri { get; }

		/// <summary>
		/// The policy to use when opening external links from the webview.
		///
		/// Defaults to opening links in an external browser.
		/// </summary>
		public ExternalLinkNavigationPolicy ExternalLinkNavigationPolicy { get; set; } = ExternalLinkNavigationPolicy.OpenInExternalBrowser;
	}
}
