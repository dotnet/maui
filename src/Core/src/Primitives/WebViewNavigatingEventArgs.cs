using System;

namespace Microsoft.Maui;

/// <summary>
/// Provides data for the <see cref="INavigatingAwareWebView.Navigating"/> event at the handler level.
/// </summary>
public class WebViewNavigatingEventArgs
{
#if WINDOWS

	/// <summary>
	/// Initializes a new instance for a navigation starting event on Windows.
	/// </summary>
	public WebViewNavigatingEventArgs(
		Uri? url,
		WebNavigationTarget target,
		global::Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs? navigationArgs,
		global::Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs? newWindowArgs)
	{
		Url = url;
		Target = target;
		NavigationArgs = navigationArgs;
		NewWindowArgs = newWindowArgs;
	}

	/// <summary>
	/// Gets the native navigation starting event args. Non-null for MainFrame and Frame navigations.
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs? NavigationArgs { get; }

	/// <summary>
	/// Gets the native new window requested event args. Non-null for NewWindow navigations.
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs? NewWindowArgs { get; }

#elif IOS || MACCATALYST

	/// <summary>
	/// Initializes a new instance for a navigation action on iOS/Mac Catalyst.
	/// </summary>
	public WebViewNavigatingEventArgs(
		Uri? url,
		WebNavigationTarget target,
		global::WebKit.WKNavigationAction navigationAction)
	{
		Url = url;
		Target = target;
		NavigationAction = navigationAction;
	}

	/// <summary>
	/// Gets the native navigation action that triggered the event.
	/// </summary>
	public global::WebKit.WKNavigationAction NavigationAction { get; }

#elif ANDROID

	/// <summary>
	/// Initializes a new instance for a navigation request on Android.
	/// </summary>
	public WebViewNavigatingEventArgs(
		Uri? url,
		WebNavigationTarget target,
		global::Android.Webkit.IWebResourceRequest? request)
	{
		Url = url;
		Target = target;
		Request = request;
	}

	/// <summary>
	/// Gets the native web resource request.
	/// </summary>
	public global::Android.Webkit.IWebResourceRequest? Request { get; }

#else

	/// <summary>
	/// Initializes a new instance of <see cref="WebViewNavigatingEventArgs"/>.
	/// </summary>
	public WebViewNavigatingEventArgs(Uri? url, WebNavigationTarget target)
	{
		Url = url;
		Target = target;
	}

#endif

	/// <summary>
	/// Gets the URI being navigated to.
	/// </summary>
	public Uri? Url { get; }

	/// <summary>
	/// Gets the frame target of the navigation.
	/// </summary>
	public WebNavigationTarget Target { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the navigation should be cancelled.
	/// </summary>
	public bool Cancel { get; set; }
}


