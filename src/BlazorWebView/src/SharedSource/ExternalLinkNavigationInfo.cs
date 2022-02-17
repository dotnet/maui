using System;

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Used to provide information about a link (<![CDATA[<a>]]>) clicked within a Blazor WebView.
	/// 
	/// `_blank` target links will always open in the default browser
	/// thus the OnExternalNavigationStarting won't be called.
	/// </summary>
	public class ExternalLinkNavigationInfo
	{
		public ExternalLinkNavigationInfo(Uri uri)
		{
			Uri = uri;
		}

		/// <summary>
		/// External <see cref="Uri">Uri</see> to be navigated to.
		/// </summary>
		public Uri Uri { get; set; }
	}
}
