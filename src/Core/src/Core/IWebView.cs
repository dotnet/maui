using System.Net;
using System.Threading.Tasks;

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
		/// When set this will act as a sync for cookies.
		/// </summary>
		CookieContainer Cookies { get; }

		/// <summary>
		/// Gets or sets the WebView's user agent string.
		/// </summary>
		string? UserAgent { get; set; }

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

		/// <summary>
		/// On platforms that support JavaScript evaluation, evaluates script.
		/// </summary>
		/// <param name="script">The script to evaluate.</param>
		/// <returns>A task that contains the result of the evaluation as a string.</returns>
		Task<string> EvaluateJavaScriptAsync(string script);

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// Raised after web navigation begins.
		/// </summary>
		bool Navigating(WebNavigationEvent evnt, string url);

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// Raised after web navigation completes.
		/// </summary>
		void Navigated(WebNavigationEvent evnt, string url, WebNavigationResult result);

		/// <summary>
		/// For internal use by the .NET MAUI platform.
		/// Raised when a WebView process ends unexpectedly.
		/// </summary>
		void ProcessTerminated(WebProcessTerminated webProcessTerminated);
	}
}