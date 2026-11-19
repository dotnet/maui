#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using WebKit;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public Task WebContentProcessTerminationRaisesProcessTerminatedAndReloadsPage() =>
		RunBlazorStartupTest(async (handler, webView) =>
		{
			var blazorWebView = Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView);
			var terminated = new TaskCompletionSource<WebViewProcessTerminatedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e) => terminated.TrySetResult(e);

			blazorWebView.ProcessTerminated += OnProcessTerminated;
			try
			{
				// Changed by hand, so only a fresh page with a fresh Blazor render shows 'Static' again.
				await WebViewHelpers.ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText = 'BeforeTermination'");

				KillWebContentProcess(webView);

				var args = await terminated.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(webView, args.PlatformArgs?.Sender);
				await WaitForControlDivAfterReload(webView, "Static");
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		});

	[Fact]
	public Task WebContentProcessTerminationRightAfterReloadDoesNotReloadAgain()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			var blazorWebView = Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView);
			var terminationCount = 0;
			var secondTermination = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e)
			{
				if (++terminationCount == 2)
				{
					secondTermination.TrySetResult();
				}
			}

			blazorWebView.ProcessTerminated += OnProcessTerminated;
			try
			{
				KillWebContentProcess(webView);
				await WaitForControlDivAfterReload(webView, "Static");

				KillWebContentProcess(webView);
				await secondTermination.Task.WaitAsync(TimeSpan.FromSeconds(10));

				var events = testLoggerProvider.GetEvents();
				Assert.Single(events, e => e.EventId.Name == "ReloadingAfterWebContentProcessTermination");
				Assert.Single(events, e => e.EventId.Name == "NotReloadingAfterWebContentProcessTermination");
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	// Ends the web content process the way the system does when it crashes or reclaims it.
	// _webProcessIdentifier is private WebKit API, used only by this test.
	static void KillWebContentProcess(WKWebView webView)
	{
		var pid = ((NSNumber)webView.ValueForKey(new NSString("_webProcessIdentifier"))).Int32Value;
		Assert.True(pid > 0, "The web view has no web content process.");
		Assert.Equal(0, kill(pid, 9));
	}

	[DllImport(ObjCRuntime.Constants.libSystemLibrary, SetLastError = true)]
	static extern int kill(int pid, int signal);

	// WebViewHelpers.WaitForControlDiv gives up on the first script error, and scripts fail while the
	// page is being reloaded.
	static async Task WaitForControlDivAfterReload(WKWebView webView, string expectedValue)
	{
		var stopwatch = Stopwatch.StartNew();
		string? lastValue = null;
		while (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
		{
			try
			{
				lastValue = await WebViewHelpers.ExecuteScriptAsync(webView,
					"document.getElementById('controlDiv') === null ? null : document.getElementById('controlDiv').innerText");
				if (lastValue == expectedValue)
				{
					return;
				}
			}
			catch (NSErrorException)
			{
			}

			await Task.Delay(250);
		}

		throw new TimeoutException($"controlDiv never became '{expectedValue}' after the reload. Last value: '{lastValue}'.");
	}
}
