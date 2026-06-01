#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_Navigating : HybridWebViewTestsBase
{
	[Fact]
	public Task NavigatingEventFiresOnMainFrameNavigation() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<HybridWebViewNavigatingEventArgs>();

			hybridWebView.Navigating += (sender, args) =>
			{
				navigatingFired.TrySetResult(args);
				args.Cancel = true; // Cancel to prevent actual navigation away
			};

			// Trigger a main frame navigation via JavaScript
			await hybridWebView.EvaluateJavaScriptAsync("navigateMainFrame('https://example.com/')");

			var result = await WaitForResult(navigatingFired);

			Assert.NotNull(result);
			Assert.Equal(WebNavigationTarget.MainFrame, result.Target);
			Assert.NotNull(result.Url);
			Assert.Equal("https://example.com/", result.Url.ToString());
		});

	[Fact]
	public Task NavigatingEventCanCancelNavigation() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<bool>();

			hybridWebView.Navigating += (sender, args) =>
			{
				args.Cancel = true;
				navigatingFired.TrySetResult(true);
			};

			// Try to navigate - should be blocked
			await hybridWebView.EvaluateJavaScriptAsync("navigateMainFrame('https://example.com/')");

			var fired = await WaitForResult(navigatingFired);
			Assert.True(fired);

			// Verify we're still on the original page by checking the DOM
			var status = await hybridWebView.EvaluateJavaScriptAsync("document.getElementById('status')?.textContent");
			Assert.Equal("ready", status);
		});

	[Fact]
	public Task NavigatingEventFiresForIframeNavigation() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<HybridWebViewNavigatingEventArgs>();

			hybridWebView.Navigating += (sender, args) =>
			{
				// Only capture iframe navigations
				if (args.Target == WebNavigationTarget.Frame)
				{
					navigatingFired.TrySetResult(args);
				}
				args.Cancel = true;
			};

			// Trigger an iframe navigation
			await hybridWebView.EvaluateJavaScriptAsync("navigateIframe('https://example.com/')");

			var result = await WaitForResult(navigatingFired);

			Assert.NotNull(result);
			Assert.Equal(WebNavigationTarget.Frame, result.Target);
			Assert.NotNull(result.Url);
			Assert.Contains("example.com", result.Url.Host);
		});

	[Fact]
	public Task NavigatingEventFiresForNewWindow() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<HybridWebViewNavigatingEventArgs>();

			hybridWebView.Navigating += (sender, args) =>
			{
				if (args.Target == WebNavigationTarget.NewWindow)
				{
					navigatingFired.TrySetResult(args);
				}
				args.Cancel = true;
			};

			// Trigger window.open
			await hybridWebView.EvaluateJavaScriptAsync("openNewWindow('https://example.com/')");

			var result = await WaitForResult(navigatingFired);

			Assert.NotNull(result);
			Assert.Equal(WebNavigationTarget.NewWindow, result.Target);
			Assert.NotNull(result.Url);
			Assert.Contains("example.com", result.Url.Host);
		});

	[Fact]
	public Task NavigatingEventProvidesCorrectUri() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<HybridWebViewNavigatingEventArgs>();

			hybridWebView.Navigating += (sender, args) =>
			{
				navigatingFired.TrySetResult(args);
				args.Cancel = true;
			};

			var expectedUrl = "https://example.com/path?query=value#fragment";
			await hybridWebView.EvaluateJavaScriptAsync($"navigateMainFrame('{expectedUrl}')");

			var result = await WaitForResult(navigatingFired);

			Assert.NotNull(result);
			Assert.NotNull(result.Url);
			Assert.Equal("example.com", result.Url.Host);
			Assert.Equal("/path", result.Url.AbsolutePath);
			Assert.Contains("query=value", result.Url.Query);
		});

	[Fact]
	public Task NavigatingEventHasPlatformArgs() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = new TaskCompletionSource<HybridWebViewNavigatingEventArgs>();

			hybridWebView.Navigating += (sender, args) =>
			{
				navigatingFired.TrySetResult(args);
				args.Cancel = true;
			};

			await hybridWebView.EvaluateJavaScriptAsync("navigateMainFrame('https://example.com/')");

			var result = await WaitForResult(navigatingFired);

			Assert.NotNull(result);
			Assert.NotNull(result.PlatformArgs);
		});

	[Fact]
	public Task NavigatingEventNotFiredForLocalAppContent() =>
		RunTest("navigationtests.html", async (hybridWebView) =>
		{
			var navigatingFired = false;

			hybridWebView.Navigating += (sender, args) =>
			{
				navigatingFired = true;
			};

			// Request a local resource (app:// scheme) - should NOT fire Navigating
			var result = await hybridWebView.EvaluateJavaScriptAsync("document.getElementById('status')?.textContent");
			Assert.Equal("ready", result);

			// Give a moment for any async events to fire
			await Task.Delay(500);

			Assert.False(navigatingFired, "Navigating event should not fire for local app content");
		});

	private static async Task<T> WaitForResult<T>(TaskCompletionSource<T> tcs, int timeoutMs = 5000)
	{
		using var cts = new CancellationTokenSource(timeoutMs);
		var registration = cts.Token.Register(() => tcs.TrySetException(new TimeoutException("Timed out waiting for Navigating event")));
		try
		{
			return await tcs.Task;
		}
		finally
		{
			await registration.DisposeAsync();
		}
	}
}
