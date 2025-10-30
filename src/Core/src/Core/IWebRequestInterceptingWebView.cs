namespace Microsoft.Maui;

public interface IWebRequestInterceptingWebView : IView
{
	/// <summary>
	/// Invoked when a web resource is requested. This event can be used to intercept requests and provide custom responses.
	/// </summary>
	/// <param name="args">The event arguments containing the request details.</param>
	/// <returns><c>true</c> if the request was handled; otherwise, <c>false</c>.</returns>
#if NETSTANDARD
	bool WebResourceRequested(WebResourceRequestedEventArgs args);
#else
	bool WebResourceRequested(WebResourceRequestedEventArgs args) => false;
#endif
}
