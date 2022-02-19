using System;

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Used to provide information about a link (<![CDATA[<a>]]>) clicked within a Blazor WebView.
	/// 
	/// `_blank` target links will always open in the default browser
	/// thus the OnExternalNavigationStarting won't be called.
	/// </summary>
	public class ExternalLinkNavigationEventArgs : EventArgs
	{
		public ExternalLinkNavigationEventArgs(Uri uri)
		{
			Uri = uri;
		}

		/// <summary>
		/// External <see cref="Uri">URI</see> to be navigated to.
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// The policy to use when opening external links from the webview.
		/// 
		/// Defaults to opening links in an external browser.
		/// </summary>
		public ExternalLinkNavigationPolicy ExternalLinkNavigationPolicy { get; set; } = ExternalLinkNavigationPolicy.OpenInExternalBrowser;
	}
}
