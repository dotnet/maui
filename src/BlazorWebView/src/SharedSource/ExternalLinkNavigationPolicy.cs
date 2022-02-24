namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Link handling policy for anchor tags <![CDATA[<a>]]> within a Blazor WebView.
	/// 
	/// Anchor tags with target="_blank" will always open in the default
	/// browser and the ExternalNavigationStarting event won't be called.
	/// </summary>
	public enum ExternalLinkNavigationPolicy
	{
		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the system default browser.
		/// </summary>
		OpenInExternalBrowser,

		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the WebView. This is not recommended unless the content of the URL is fully trusted.
		/// </summary>
		OpenInWebView,

		/// <summary>
		/// Cancels the current navigation attempt to an external link.
		/// </summary>
		CancelNavigation
	}
}
