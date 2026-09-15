#nullable enable
using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using WebKit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_MessageOrigins
{
	private static partial async Task LoadOtherOriginDocumentAsync(
		HybridWebViewHandler handler,
		HybridWebView hybridWebView,
		string html)
	{
		var navigationDelegate = new NavigationDelegate();
		var previousNavigationDelegate = handler.PlatformView.NavigationDelegate;
		handler.PlatformView.NavigationDelegate = navigationDelegate;

		try
		{
			handler.PlatformView.LoadHtmlString(html, new NSUrl(OtherOrigin));
			await navigationDelegate.NavigationCompleted.Task.WaitAsync(OperationWaitTimeout);
		}
		finally
		{
			handler.PlatformView.NavigationDelegate = previousNavigationDelegate;
		}
	}

	sealed class NavigationDelegate : WKNavigationDelegate
	{
		public TaskCompletionSource NavigationCompleted { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public override void DidFinishNavigation(WKWebView webView, WKNavigation? navigation) =>
			NavigationCompleted.TrySetResult();

		public override void DidFailNavigation(WKWebView webView, WKNavigation? navigation, NSError error) =>
			NavigationCompleted.TrySetException(new InvalidOperationException(error.LocalizedDescription));

		public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation? navigation, NSError error) =>
			NavigationCompleted.TrySetException(new InvalidOperationException(error.LocalizedDescription));
	}
}
