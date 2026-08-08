using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;
#if ANDROID
using PlatformWebView = Android.Webkit.WebView;
#elif IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
#elif WINDOWS
using PlatformWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using PlatformWebView = System.Object;
#endif

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	Task RunBlazorStartupTest(
		Func<PlatformWebView, Task> test,
		TestLoggerProvider? testLoggerProvider = null,
		Type? componentType = null,
		string? indexHtml = null,
		Func<PlatformWebView, Task>? waitForReady = null)
	{
		return RunBlazorStartupTest(
			(_, platformWebView) => test(platformWebView),
			testLoggerProvider,
			componentType,
			indexHtml,
			waitForReady);
	}

	async Task RunBlazorStartupTest(
		Func<BlazorWebViewHandler, PlatformWebView, Task> test,
		TestLoggerProvider? testLoggerProvider = null,
		Type? componentType = null,
		string? indexHtml = null,
		Func<PlatformWebView, Task>? waitForReady = null)
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
			if (testLoggerProvider is not null)
			{
				appBuilder.Services.AddLogging(logging =>
				{
					logging.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);
					logging.AddProvider(testLoggerProvider);
				});
			}
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", indexHtml ?? TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = componentType ?? typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			if (waitForReady is null)
			{
				await WebViewHelpers.WaitForWebViewReady(platformWebView);
				await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "Static");
			}
			else
			{
				await waitForReady(platformWebView);
			}

			await test(bwvHandler, platformWebView);
		});
	}

	[Fact]
	public Task BlazorStartupSetsUpWindowExternal() => RunBlazorStartupTest(async platformWebView =>
	{
		// window.external should have sendMessage and receiveMessage functions on all platforms.
		// Use .toString() to normalize boolean results across platforms (iOS returns "1" for true).
		var hasSendMessage = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "(typeof window.external.sendMessage === 'function').toString()");
		var hasReceiveMessage = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "(typeof window.external.receiveMessage === 'function').toString()");

		Assert.True(hasSendMessage == "true" || hasSendMessage == "\"true\"", $"Expected sendMessage to be a function, got: {hasSendMessage}");
		Assert.True(hasReceiveMessage == "true" || hasReceiveMessage == "\"true\"", $"Expected receiveMessage to be a function, got: {hasReceiveMessage}");
	});

#if ANDROID
	[Fact]
	public Task BlazorStartupSetsStartingAndStartedFlags() => RunBlazorStartupTest(async platformWebView =>
	{
		// After Blazor is fully started, both flags should be true
		var startingValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__BlazorStarting === true");
		var startedValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__BlazorStarted === true");

		Assert.Equal("true", startingValue);
		Assert.Equal("true", startedValue);
	});

	[Fact]
	public Task BlazorStartupCapturesNativePort() => RunBlazorStartupTest(async platformWebView =>
	{
		// The native port should have been captured during startup
		var hasNativePort = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__nativePort !== null && window.__nativePort !== undefined");

		Assert.Equal("true", hasNativePort);
	});

	[Fact]
	public Task BlazorStartupScriptIsIdempotent()
	{
		var testLoggerProvider = new TestLoggerProvider();
		return RunBlazorStartupTest(async (bwvHandler, platformWebView) =>
		{
			var setupEventsBefore = testLoggerProvider.GetEvents().Count(
				logEvent => logEvent.EventId.Id == 40 && logEvent.EventId.Name == "BlazorStartupScriptsSubmitted");
			Assert.Equal(1, setupEventsBefore);

			// Store the original native port reference
			await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__originalNativePort = window.__nativePort");

			var webViewClientField = typeof(BlazorWebViewHandler)
				.GetField("_webViewClient", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(webViewClientField);

			var webViewClient = webViewClientField.GetValue(bwvHandler) as global::Android.Webkit.WebViewClient;
			Assert.NotNull(webViewClient);

			// Simulate a duplicate OnPageFinished callback so the production startup path runs again.
			webViewClient.OnPageFinished(platformWebView, platformWebView.Url);
			await WebViewHelpers.ExecuteScriptAsync(platformWebView, "true");

			// Verify the native port is still the same reference and no second channel setup was submitted.
			var portUnchanged = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__nativePort === window.__originalNativePort");
			Assert.Equal("true", portUnchanged);

			var setupEventsAfter = testLoggerProvider.GetEvents().Count(
				logEvent => logEvent.EventId.Id == 40 && logEvent.EventId.Name == "BlazorStartupScriptsSubmitted");
			Assert.Equal(setupEventsBefore, setupEventsAfter);
		}, testLoggerProvider);
	}

	[Fact]
	public Task BlazorStartupRejectionPreservesLiveBridge()
	{
		var rejectingIndexHtml = TestStaticFilesContents.DefaultMauiIndexHtmlContent.Replace(
			"</body>",
			"""
			    <script>
			        (function () {
			            const originalStart = Blazor.start.bind(Blazor);
			            Blazor.start = function (options) {
			                return originalStart(options).then(function (value) {
			                    window.__wrappedBlazorStartRejected = true;
			                    throw new Error('Deliberate post-start rejection');
			                });
			            };
			        })();
			    </script>
			</body>
			""",
			StringComparison.Ordinal);

		return RunBlazorStartupTest(
			async platformWebView =>
			{
				var nativePortPresent = await WebViewHelpers.ExecuteScriptAsync(
					platformWebView,
					"window.__nativePort !== null && window.__nativePort !== undefined");
				Assert.Equal("true", nativePortPresent);

				await WebViewHelpers.ExecuteScriptAsync(
					platformWebView,
					"document.getElementById('incrementButton').click()");
				await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "1");
			},
			componentType: typeof(TestComponent1),
			indexHtml: rejectingIndexHtml,
			waitForReady: async platformWebView =>
			{
				await WebViewHelpers.WaitForCondition(platformWebView, "window.__wrappedBlazorStartRejected === true");
				await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "0");
			});
	}

	[Fact]
	public Task BlazorMessageDispatchOnlyProcessesNativeSourceMessages() => RunBlazorStartupTest(async platformWebView =>
	{
		// Verify the message listener is working by checking that the native bridge
		// dispatched messages during startup (Blazor is running, which means
		// null-source native messages were processed correctly).
		var blazorRunning = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.Blazor != null");
		Assert.Equal("true", blazorRunning);

		// Set up: a callback counter for native dispatch, and a separate test-only
		// listener that gives us a positive signal when the message event fires.
		await WebViewHelpers.ExecuteScriptAsync(platformWebView, @"
			window.__nativeCallbackCount = 0;
			window.__testMessageSeen = false;
			window.external.receiveMessage(function(msg) { window.__nativeCallbackCount++; });
			window.addEventListener('message', function(event) {
				if (event.data === 'test-from-js') { window.__testMessageSeen = true; }
			}, false);
		");

		// Send a message from JS using window.postMessage — these have event.source
		// set to the current window (non-null), so they go through a different path
		// than native PostWebMessage messages (which have null source).
		await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.postMessage('test-from-js', '*')");

		// Wait for positive confirmation that the message event was processed.
		// Our test listener sets __testMessageSeen when it sees the message,
		// which proves the event loop has processed it and the production
		// listener also had its chance to run.
		await WebViewHelpers.WaitForCondition(platformWebView, "window.__testMessageSeen === true");

		// Now we can safely check: the callback count should still be zero because
		// only native-sourced messages (null source) are dispatched to the callback.
		var count = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__nativeCallbackCount");
		Assert.Equal("0", count);
	});
#endif
}
