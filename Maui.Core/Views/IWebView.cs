namespace System.Maui
{
	public interface IWebView : IView
	{
		bool CanGoBack { get; set; }
		bool CanGoForward { get; set; }
		WebViewSource Source { get; set; }

		event EventHandler<EvalRequested> EvalRequested;
		event EvaluateJavaScriptDelegate EvaluateJavaScriptRequested;
		event EventHandler GoBackRequested;
		event EventHandler GoForwardRequested;
		event EventHandler ReloadRequested;

		void Navigated(WebNavigatedEventArgs args);
		void Navigating(WebNavigatingEventArgs args);
	}
}