#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="WebView.Navigated"/> event, raised after navigation completes.
	/// </summary>
	public class WebNavigatedEventArgs : WebNavigationEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebNavigatedEventArgs"/> class.
		/// </summary>
		/// <param name="navigationEvent">The type of navigation event.</param>
		/// <param name="source">The source of the web view content.</param>
		/// <param name="url">The URL that was navigated to.</param>
		/// <param name="result">The result of the navigation.</param>
		public WebNavigatedEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, WebNavigationResult result) : base(navigationEvent, source, url)
		{
			Result = result;
		}

		/// <summary>
		/// Gets the result of the navigation operation.
		/// </summary>
		public WebNavigationResult Result { get; private set; }
	}
}