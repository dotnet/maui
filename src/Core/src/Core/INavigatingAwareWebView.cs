namespace Microsoft.Maui;

/// <summary>
/// Interface for web views that support navigation interception.
/// </summary>
public interface INavigatingAwareWebView : IView
{
	/// <summary>
	/// Invoked when the web view is about to navigate. This event can be used to intercept and cancel navigations.
	/// </summary>
	/// <param name="args">The event arguments containing the navigation details.</param>
	/// <returns><c>true</c> if the navigation should be cancelled; otherwise, <c>false</c>.</returns>
#if NETSTANDARD
	bool Navigating(WebViewNavigatingEventArgs args);
#else
	bool Navigating(WebViewNavigatingEventArgs args) => false;
#endif
}
