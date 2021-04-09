namespace Microsoft.Maui.Controls
{
	public partial class WebView : IWebView
	{
		IWebViewSource IWebView.Source { get; set; } 
	}
}