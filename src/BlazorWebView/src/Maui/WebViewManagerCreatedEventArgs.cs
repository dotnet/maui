namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class WebViewManagerCreatedEventArgs
	{
		public WebViewManagerCreatedEventArgs(WebViewManager webViewManager)
		{
			WebViewManager = webViewManager;
		}

		public WebViewManager WebViewManager { get; }
	}
}
