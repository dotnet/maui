namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Link handling policy for anchor tags <![CDATA[<a>]]> within a Blazor WebView.
	/// 
	/// `_blank` target links will always open in the default browser
	/// regardless of the policy.
	/// </summary>
	public enum ExternalLinkNavigationPolicy
	{
		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the system default browser.
		/// </summary>
		OpenInExternalBrowser,

		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the WebView. This is not recommended.
		/// </summary>
		OpenInWebView,

		/// <summary>
		/// Cancels the current navigation attempt to an external link.
		/// </summary>
		CancelNavigation
	}
}
