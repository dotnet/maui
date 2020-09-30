namespace Xamarin.Forms
{
	public class WebNavigatedEventArgs : WebNavigationEventArgs
	{
		public WebNavigatedEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, WebNavigationResult result) : base(navigationEvent, source, url)
		{
			Result = result;
		}

		public WebNavigationResult Result { get; private set; }
	}
}