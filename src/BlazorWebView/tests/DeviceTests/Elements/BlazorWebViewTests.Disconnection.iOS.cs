using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNetCore.Components.WebView.Maui;
using WebKit;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public Task SchemeRequestsAfterDisconnectionAreIgnored() =>
		RunBlazorStartupTest((handler, webView) =>
		{
			var schemeHandler = webView.Configuration.GetUrlSchemeHandler("app");
			Assert.NotNull(schemeHandler);

			((IElementHandler)handler).DisconnectHandler();

			using var task = new DisconnectedUrlSchemeTask();
			schemeHandler.StartUrlSchemeTask(webView, task);
			return Task.CompletedTask;
		});

	[Fact]
	public Task SchemeRequestsDisconnectingDuringInterceptionAreIgnored() =>
		RunBlazorStartupTest((handler, webView) =>
		{
			var schemeHandler = webView.Configuration.GetUrlSchemeHandler("app");
			Assert.NotNull(schemeHandler);
			var intercepted = false;
			Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView).WebResourceRequested += (_, _) =>
			{
				intercepted = true;
				((IElementHandler)handler).DisconnectHandler();
			};

			using var task = new DisconnectedUrlSchemeTask();
			schemeHandler.StartUrlSchemeTask(webView, task);
			Assert.True(intercepted);
			return Task.CompletedTask;
		});

	[Fact]
	public Task NavigationAfterDisconnectionIsCancelled() =>
		RunBlazorStartupTest(async (handler, webView) =>
		{
			var navigationDelegate = webView.NavigationDelegate;
			Assert.NotNull(navigationDelegate);
			var completion = new TaskCompletionSource<WKNavigationActionPolicy>();
			using var disconnectingDelegate = new DisconnectingNavigationDelegate((action, decisionHandler) =>
			{
				decisionHandler(WKNavigationActionPolicy.Cancel);
				((IElementHandler)handler).DisconnectHandler();
				WKNavigationActionPolicy? policy = null;
				var exception = Record.Exception(() =>
					navigationDelegate.DecidePolicy(webView, action, value => policy = value));
				if (exception is not null)
					completion.TrySetException(exception);
				else if (policy.HasValue)
					completion.TrySetResult(policy.Value);
				else
					completion.TrySetException(new InvalidOperationException("The navigation policy callback was not invoked."));
			});

			webView.NavigationDelegate = disconnectingDelegate;
			using var url = new NSUrl("app://0.0.0.1/late-navigation");
			using var request = new NSUrlRequest(url);
			webView.LoadRequest(request);
			Assert.Equal(WKNavigationActionPolicy.Cancel, await completion.Task.WaitAsync(TimeSpan.FromSeconds(10)));
		});

	sealed class DisconnectingNavigationDelegate(Action<WKNavigationAction, Action<WKNavigationActionPolicy>> onNavigation) : WKNavigationDelegate
	{
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) =>
			onNavigation(navigationAction, decisionHandler);
	}

	sealed class DisconnectedUrlSchemeTask : NSObject, IWKUrlSchemeTask
	{
		public NSUrlRequest Request { get; } = new NSUrlRequest(new NSUrl("app://0.0.0.1/index.html"));

		public void DidReceiveResponse(NSUrlResponse response) => throw new InvalidOperationException("A disconnected request must not receive a response.");

		public void DidReceiveData(NSData data) => throw new InvalidOperationException("A disconnected request must not receive data.");

		public void DidFinish() => throw new InvalidOperationException("A disconnected request must not finish.");

		public void DidFailWithError(NSError error) => throw new InvalidOperationException("A disconnected request must not be completed.");

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Request.Dispose();
			base.Dispose(disposing);
		}
	}
}
