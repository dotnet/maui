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
		/// Evaluates the script that is specified by script.
		/// </summary>
		/// <param name="script">A script to evaluate.</param>
		void Eval(string script);
	}
}