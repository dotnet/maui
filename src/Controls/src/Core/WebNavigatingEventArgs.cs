namespace Microsoft.Maui.Controls
{
	public class WebNavigatingEventArgs : DeferrableEventArgs, IWebNavigationEventArgs
	{
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, bool canCancel)
			: base(canCancel)
		{
			this.NavigationEvent = navigationEvent;
			this.Source = source;
			this.Url = url;
		}

		public WebNavigationEvent NavigationEvent { get; private set; }

		public WebViewSource Source { get; private set; }

		public string Url { get; private set; }
	}
}