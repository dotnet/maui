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
		/// Gets a value that indicates whether the user can navigate to previous pages.
		/// </summary>
		bool CanGoBack { get; set; }

		/// <summary>
		/// Gets a value that indicates whether the user can navigate forward.
		/// </summary>
		bool CanGoForward { get; set; }

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

		/// <summary>
		/// Evaluates the script that is specified by script.
		/// </summary>
		/// <param name="script">A script to evaluate.</param>
		void Eval(string script);
	}
}