#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="WebView.Navigating"/> event, raised before navigation begins.
	/// </summary>
	public class WebNavigatingEventArgs : WebNavigationEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebNavigatingEventArgs"/> class.
		/// </summary>
		/// <param name="navigationEvent">The type of navigation event.</param>
		/// <param name="source">The source of the web view content.</param>
		/// <param name="url">The URL being navigated to.</param>
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url) : base(navigationEvent, source, url)
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether to cancel the navigation.
		/// </summary>
		public bool Cancel { get; set; }
	}
}