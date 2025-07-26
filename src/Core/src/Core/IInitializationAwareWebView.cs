namespace Microsoft.Maui;

public interface IInitializationAwareWebView : IView
{
	/// <summary>
	/// Invoked when web view initialization is starting. This event allows the application to perform additional configuration.
	/// </summary>
#if NETSTANDARD
	void WebViewInitializationStarted(WebViewInitializationStartedEventArgs args);
#else
	void WebViewInitializationStarted(WebViewInitializationStartedEventArgs args) { }
#endif

	/// <summary>
	/// Invoked when the web view has been initialized.
	/// </summary>
#if NETSTANDARD
	void WebViewInitializationCompleted(WebViewInitializationCompletedEventArgs args);
#else
	void WebViewInitializationCompleted(WebViewInitializationCompletedEventArgs args) { }
#endif
}
