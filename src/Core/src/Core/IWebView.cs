namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that presents HTML content.
	/// </summary>
	public interface IWebView : IView
	{
		/// <summary>
		/// Provide the data for a WebView.
		/// </summary>
		IWebViewSource Source { get; }

		/// <summary>
		/// Navigates to the previous page.
		/// </summary>
		void GoBack();

		/// <summary>
		/// Navigates to the next page in the list of visited pages.
		/// </summary>
		void GoForward();
		
		/// <summary>
		/// Reload the current content.
		/// </summary>
		void Reload();
	}
}