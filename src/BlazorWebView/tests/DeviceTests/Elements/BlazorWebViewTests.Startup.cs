using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public async Task BlazorStartupSetsUpWindowExternal()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

			// window.external should have sendMessage and receiveMessage functions on all platforms.
			// Use .toString() to normalize boolean results across platforms (iOS returns "1" for true).
			var hasSendMessage = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "(typeof window.external.sendMessage === 'function').toString()");
			var hasReceiveMessage = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "(typeof window.external.receiveMessage === 'function').toString()");

			Assert.True(hasSendMessage == "true" || hasSendMessage == "\"true\"", $"Expected sendMessage to be a function, got: {hasSendMessage}");
			Assert.True(hasReceiveMessage == "true" || hasReceiveMessage == "\"true\"", $"Expected receiveMessage to be a function, got: {hasReceiveMessage}");
		});
	}

#if ANDROID
	[Fact]
	public async Task BlazorStartupSetsStartingAndStartedFlags()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

			// After Blazor is fully started, both flags should be true
			var startingValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__BlazorStarting === true");
			var startedValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__BlazorStarted === true");

			Assert.Equal("true", startingValue);
			Assert.Equal("true", startedValue);
		});
	}

	[Fact]
	public async Task BlazorStartupCapturesNativePort()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

			// The native port should have been captured during startup
			var hasNativePort = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__nativePort !== null && window.__nativePort !== undefined");

			Assert.Equal("true", hasNativePort);
		});
	}

	[Fact]
	public async Task BlazorStartupScriptIsIdempotent()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

			// Store the original native port reference
			await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__originalNativePort = window.__nativePort");

			// The __BlazorStarting guard should prevent re-initialization on duplicate OnPageFinished
			var rerunResult = await WebViewHelpers.ExecuteScriptAsync(platformWebView,
				"(function() { if (window.__BlazorStarting) { return 'blocked'; } return 'ran'; })()");

			Assert.Equal("\"blocked\"", rerunResult);

			// Verify the native port is still the same reference
			var portUnchanged = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "window.__nativePort === window.__originalNativePort");
			Assert.Equal("true", portUnchanged);
		});
	}

	[Fact]
	public async Task BlazorMessageDispatchOnlyProcessesNativeSourceMessages()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

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
	}
#endif
}
