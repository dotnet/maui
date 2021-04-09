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
		IWebViewSource Source { get; set; }
	}
}