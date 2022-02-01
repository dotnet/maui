namespace Microsoft.Maui.Controls
{
	public class WebNavigatingEventArgs : WebNavigationEventArgs
	{
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, IWebViewSource source, string url) : base(navigationEvent, source, url)
		{
		}

		public bool Cancel { get; set; }
	}
}