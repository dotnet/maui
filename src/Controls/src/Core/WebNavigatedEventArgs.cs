namespace Microsoft.Maui.Controls
{
	public class WebNavigatedEventArgs : IWebNavigationEventArgs
	{
		public WebNavigatedEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, WebNavigationResult result)
		{
			this.Result = result;
			this.NavigationEvent = navigationEvent;
			this.Source = source;
			this.Url = url;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatedEventArgs.xml" path="//Member[@MemberName='Result']/Docs" />
		public WebNavigationResult Result { get; private set; }

		public WebNavigationEvent NavigationEvent { get; private set; }

		public WebViewSource Source { get; private set; }

		public string Url { get; private set; }
	}
}