#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests;
using WebKit;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	const string ReloadingEventName = "ReloadingAfterWebContentProcessTermination";
	const string NotReloadingEventName = "NotReloadingAfterWebContentProcessTermination";
	const string DeferringEventName = "DeferringReloadAfterWebContentProcessTermination";

	[Fact]
	public Task WebContentProcessTerminationRaisesProcessTerminatedAndReloadsVisiblePage()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			var blazorWebView = Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView);
			var terminated = new TaskCompletionSource<WebViewProcessTerminatedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e) => terminated.TrySetResult(e);

			blazorWebView.ProcessTerminated += OnProcessTerminated;
			try
			{
				await webView.AttachAndRun(async () =>
				{
					// Changed by hand, so only a fresh page with a fresh Blazor render shows 'Static' again.
					await WebViewHelpers.ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText = 'BeforeTermination'");

					KillWebContentProcess(webView);

					var args = await terminated.Task.WaitAsync(TimeSpan.FromSeconds(10));
					Assert.Same(webView, args.PlatformArgs?.Sender);
					await WaitForControlDivAfterReload(webView, "Static");
				});

				var events = testLoggerProvider.GetEvents();
				Assert.Single(events, e => e.EventId.Name == ReloadingEventName);
				Assert.DoesNotContain(events, e => e.EventId.Name == DeferringEventName);
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	[Fact]
	public Task WebContentProcessTerminationReloadsHiddenPageOnceItIsInAWindow()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			var blazorWebView = Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView);
			var terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e) => terminated.TrySetResult();

			blazorWebView.ProcessTerminated += OnProcessTerminated;
			try
			{
				await WebViewHelpers.ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText = 'BeforeTermination'");

				// The web view has not been added to a window, so nobody can see it.
				KillWebContentProcess(webView);
				await terminated.Task.WaitAsync(TimeSpan.FromSeconds(10));

				var events = testLoggerProvider.GetEvents();
				Assert.Single(events, e => e.EventId.Name == DeferringEventName);
				Assert.DoesNotContain(events, e => e.EventId.Name == ReloadingEventName);

				await webView.AttachAndRun(() => WaitForControlDivAfterReload(webView, "Static"));

				Assert.Single(testLoggerProvider.GetEvents(), e => e.EventId.Name == ReloadingEventName);
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	[Fact]
	public Task WebContentProcessTerminationRightAfterReloadDoesNotReloadAgain()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			// Never advanced, so the second termination always comes less than 30 seconds after the reload.
			handler.TimeProvider = new ManualTimeProvider();

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
				await webView.AttachAndRun(async () =>
				{
					KillWebContentProcess(webView);
					await WaitForControlDivAfterReload(webView, "Static");
					await WaitForPageLoaded(webView);

					KillWebContentProcess(webView);
					await secondTermination.Task.WaitAsync(TimeSpan.FromSeconds(10));
				});

				var events = testLoggerProvider.GetEvents();
				Assert.Single(events, e => e.EventId.Name == ReloadingEventName);
				Assert.Single(events, e => e.EventId.Name == NotReloadingEventName);
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	[Fact]
	public Task WebContentProcessTerminationLongAfterReloadReloadsAgain()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			var timeProvider = new ManualTimeProvider();
			handler.TimeProvider = timeProvider;

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
				await webView.AttachAndRun(async () =>
				{
					KillWebContentProcess(webView);
					await WaitForControlDivAfterReload(webView, "Static");
					await WaitForPageLoaded(webView);

					timeProvider.Advance(WebContentProcessReloader.RecentReloadInterval);

					await WebViewHelpers.ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText = 'BeforeTermination'");
					KillWebContentProcess(webView);
					await secondTermination.Task.WaitAsync(TimeSpan.FromSeconds(10));
					await WaitForControlDivAfterReload(webView, "Static");
				});

				var events = testLoggerProvider.GetEvents();
				Assert.Equal(2, Array.FindAll(events, e => e.EventId.Name == ReloadingEventName).Length);
				Assert.DoesNotContain(events, e => e.EventId.Name == NotReloadingEventName);
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	[Fact]
	public Task WebContentProcessTerminationBeforeFirstPageLoadsLoadsThatPageAgain()
	{
		var testLoggerProvider = new TestLoggerProvider();
		BlazorWebView? blazorWebView = null;
		var hasKilledWebContentProcess = false;
		var hadPageAtTermination = true;

		// UrlLoading is raised while WebKit decides whether to start the first navigation, before anything is loaded.
		// It runs inside a WebKit callback, where a failed assertion would crash the test app, so the test body asserts.
		void OnUrlLoading(object? sender, UrlLoadingEventArgs e)
		{
			if (!hasKilledWebContentProcess && blazorWebView?.Handler?.PlatformView is WKWebView)
			{
				hasKilledWebContentProcess = TryKillWebContentProcess((WKWebView)blazorWebView.Handler.PlatformView);
			}
		}

		void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e) =>
			hadPageAtTermination = ((WKWebView)e.PlatformArgs!.Sender).BackForwardList.CurrentItem is not null;

		return RunBlazorStartupTest(
			async (handler, webView) =>
			{
				try
				{
					await webView.AttachAndRun(() => WaitForControlDivAfterReload(webView, "Static"));

					Assert.True(hasKilledWebContentProcess);
					Assert.False(hadPageAtTermination, "The web content process ended after the first page was loaded, so this test did not cover a termination with no page to reload.");
					Assert.Single(testLoggerProvider.GetEvents(), e => e.EventId.Name == ReloadingEventName);
				}
				finally
				{
					blazorWebView!.UrlLoading -= OnUrlLoading;
					blazorWebView.ProcessTerminated -= OnProcessTerminated;
				}
			},
			testLoggerProvider,
			waitForReady: _ => Task.CompletedTask,
			configureBlazorWebView: bwv =>
			{
				blazorWebView = bwv;
				bwv.UrlLoading += OnUrlLoading;
				bwv.ProcessTerminated += OnProcessTerminated;
			});
	}

	[Fact]
	public Task WebContentProcessTerminationAfterThePageNavigatedByItselfReloadsTheNewPage()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (handler, webView) =>
		{
			var blazorWebView = Assert.IsAssignableFrom<BlazorWebView>(handler.VirtualView);
			var terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e) => terminated.TrySetResult();

			// The startup page was asked for with LoadRequest. This one is not: the page navigates to it by itself.
			var navigatedToUrl = BlazorWebViewHandler.AppOrigin + "PageTheWebViewNavigatedTo";

			blazorWebView.ProcessTerminated += OnProcessTerminated;
			try
			{
				await webView.AttachAndRun(async () =>
				{
					await WebViewHelpers.ExecuteScriptAsync(webView,
						$"document.getElementById('controlDiv').innerText = 'BeforeNavigation'; setTimeout(function () {{ location.href = '{navigatedToUrl}'; }}); 'navigating'");
					await WaitForControlDivAfterReload(webView, "Static");
					Assert.Equal(navigatedToUrl, await WebViewHelpers.ExecuteScriptAsync(webView, "location.href"));
					await WaitForPageLoaded(webView);

					await WebViewHelpers.ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText = 'BeforeTermination'");
					KillWebContentProcess(webView);
					await terminated.Task.WaitAsync(TimeSpan.FromSeconds(10));
					await WaitForControlDivAfterReload(webView, "Static");

					Assert.Equal(navigatedToUrl, await WebViewHelpers.ExecuteScriptAsync(webView, "location.href"));
				});

				Assert.Single(testLoggerProvider.GetEvents(), e => e.EventId.Name == ReloadingEventName);
			}
			finally
			{
				blazorWebView.ProcessTerminated -= OnProcessTerminated;
			}
		}, testLoggerProvider);
	}

	// Ends the web content process the way the system does when it crashes or reclaims it.
	// _webProcessIdentifier is private WebKit API, used only by this test.
	static void KillWebContentProcess(WKWebView webView) =>
		Assert.True(TryKillWebContentProcess(webView), "The web view has no web content process to end.");

	static bool TryKillWebContentProcess(WKWebView webView)
	{
		var pid = ((NSNumber)webView.ValueForKey(new NSString("_webProcessIdentifier"))).Int32Value;
		return pid > 0 && kill(pid, 9) == 0;
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

	// Blazor can render before the page finishes loading, and the 30 seconds after a reload only start counting once
	// it has.
	static async Task WaitForPageLoaded(WKWebView webView)
	{
		var stopwatch = Stopwatch.StartNew();
		while (webView.IsLoading)
		{
			if (stopwatch.Elapsed > TimeSpan.FromSeconds(30))
			{
				throw new TimeoutException("The reloaded page never finished loading.");
			}

			await Task.Delay(50);
		}
	}

	sealed class ManualTimeProvider : TimeProvider
	{
		long _timestamp;

		public override long TimestampFrequency => TimeSpan.TicksPerSecond;

		public override long GetTimestamp() => _timestamp;

		public void Advance(TimeSpan duration) => _timestamp += duration.Ticks;
	}
}
