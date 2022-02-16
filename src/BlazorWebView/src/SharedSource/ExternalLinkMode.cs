namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Link handling mode for anchor tags <![CDATA[<a>]]> within a Blazor WebView.
	/// 
	/// `_blank` target links will always open in the default browser
	/// regardless of this setting.
	/// </summary>
	public enum ExternalLinkMode
	{
		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the system default browser.
		/// </summary>
		OpenInExternalBrowser,

		/// <summary>
		/// Opens anchor tags <![CDATA[<a>]]> in the WebView. This is not recommended.
		/// </summary>
		OpenInWebView
	}
}
