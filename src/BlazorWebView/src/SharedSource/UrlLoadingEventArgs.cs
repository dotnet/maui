using System;

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Used to provide information about a link (<![CDATA[<a>]]>) clicked within a Blazor WebView.
	/// <para>
	/// Anchor tags with target="_blank" will always open in the default
	/// browser and the UrlLoading event won't be called.
	/// </para>
	/// </summary>
	public class UrlLoadingEventArgs : EventArgs
	{
		internal static UrlLoadingEventArgs CreateWithDefaultLoadingStrategy(Uri urlToLoad, Uri appOriginUri)
		{
			var strategy = appOriginUri.IsBaseOf(urlToLoad) ?
				UrlLoadingStrategy.OpenInWebView :
				UrlLoadingStrategy.OpenExternally;

			return new(urlToLoad, strategy);
		}

		private UrlLoadingEventArgs(Uri uri, UrlLoadingStrategy urlLoadingStrategy)
		{
			Uri = uri;
			UrlLoadingStrategy = urlLoadingStrategy;
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
		public UrlLoadingStrategy UrlLoadingStrategy { get; set; } = UrlLoadingStrategy.OpenExternally;
	}
}
