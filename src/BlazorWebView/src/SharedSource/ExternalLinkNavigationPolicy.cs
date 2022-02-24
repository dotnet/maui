namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// External link handling policy for anchor tags <![CDATA[<a>]]> within a Blazor WebView.
	/// 
	/// Anchor tags with target="_blank" will always open in the default
	/// browser and the ExternalNavigationStarting event won't be called.
	/// </summary>
	public enum ExternalLinkNavigationPolicy
	{
		/// <summary>
		/// Allows navigation to external links using the system default browser.
		/// This is the default navigation policy.
		/// </summary>
		OpenInExternalBrowser,

		/// <summary>
		/// Allows navigation to external links within the Blazor WebView.
		/// This navigation policy can introduce security concerns and should not be enabled unless you can ensure all external links are fully trusted.
		/// </summary>
		InsecureOpenInWebView,

		/// <summary>
		/// Cancels the current navigation attempt to an external link.
		/// </summary>
		CancelNavigation
	}
}
