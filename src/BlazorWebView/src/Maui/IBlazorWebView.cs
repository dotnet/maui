using System;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public interface IBlazorWebView : IView
	{
		string? HostPage { get; set; }
		RootComponentsCollection RootComponents { get; }
		event EventHandler<WebViewManagerCreatedEventArgs>? WebViewManagerCreated;
		WebViewManager? WebViewManager { get; }
	}
}
