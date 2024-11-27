﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebViewAppShared;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements
{
	[Category(TestCategory.BlazorWebView)]
	public class BlazorWebViewTests : Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
	{
		[Fact]
		public async Task BasicRazorComponentClick()
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
			bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(TestComponent1), Selector = "#app", });

			await InvokeOnMainThreadAsync(async () =>
			{
				var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
				var platformWebView = bwvHandler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);

				// Click a button in a Razor component 3 times
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "0");

				var c1 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "1");

				var c2 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "2");

				var c3 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "3");

				// And the counter value should increment from 0 to 3.
				var actualFinalCounterValue = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('counterValue').innerText");
				actualFinalCounterValue = actualFinalCounterValue.Trim('\"'); // some platforms return quoted values, so we trim them
				Assert.Equal("3", actualFinalCounterValue);
			});
		}

		[Fact]
		public async Task BlazorWebViewLogsRequests()
		{
			var testLoggerProvider = new TestLoggerProvider();
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
				appBuilder.Services.AddLogging(c =>
				{
					// Enable maximum logging for BlazorWebView
					c.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);

					c.AddProvider(testLoggerProvider);
				});
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

				// Wait for the no-op component to load
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");
			});

			var events = testLoggerProvider.GetEvents();

			// Here we choose an arbitrary subset of logs to verify. We could check every single one, but
			// it's different on each platform, and subject to change in the future. Less is more.
			Assert.Equal(1, events.Count(c => c.EventId.Id == 0 && c.LogLevel == LogLevel.Debug && c.EventId.Name == "NavigatingToUri"));
			Assert.Equal(1, events.Count(c => c.EventId.Id == 4 && c.LogLevel == LogLevel.Debug && c.EventId.Name == "HandlingWebRequest" && c.Message.Contains("/_framework/blazor.webview.js", System.StringComparison.Ordinal)));
		}


		[Fact]
		public async Task BlazorWebViewUsesStartPath()
		{
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
			});

			var bwv = new BlazorWebViewWithCustomFiles
			{
				StartPath = "CustomStart/SomeData",
				HostPage = "wwwroot/index.html",
				CustomFiles = new Dictionary<string, string>
				{
					{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				},
			};
			bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(RouterComponent), Selector = "#app", });

			await InvokeOnMainThreadAsync(async () =>
			{
				var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
				var platformWebView = bwvHandler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);

				// Wait for the component to load
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Found the start path with: 'SomeData'");
			});
		}

		[Fact]
		public async Task BlazorWebViewDispatchGetsScopedServices()
		{
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
				appBuilder.Services.AddScoped<ExampleJsInterop>();
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

				// Wait for the no-op component to load
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

				// Use BlazorWebView.TryDispatchAsync to access scoped services
				var calledWorkItem = await bwv.TryDispatchAsync(async services =>
				{
					var jsInterop = services.GetRequiredService<ExampleJsInterop>();
					await jsInterop.UpdateControlDiv("some new value");
				});

				Assert.True(calledWorkItem);

				// Wait for the no-op component to show the new value
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "some new value");
			});
		}

		[Fact]
		public async Task BlazorWebViewDispatchBeforeRunningReturnsFalse()
		{
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
				appBuilder.Services.AddScoped<ExampleJsInterop>();
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
				// Try to dispatch before the MAUI handler is created
				var calledWorkItem = await bwv.TryDispatchAsync(_ => throw new NotImplementedException());

				Assert.False(calledWorkItem);
			});
		}

		[Fact]
		public async Task BlazorWebViewWithoutDispatchFailsToGetScopedServices()
		{
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
				appBuilder.Services.AddScoped<ExampleJsInterop>();
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

				// Wait for the no-op component to load
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Static");

				// DON'T use BlazorWebView.Dispatch to access scoped services; instead, just try to get it directly (and fail)
				var jsInterop = bwv.Handler.MauiContext.Services.GetRequiredService<ExampleJsInterop>();

				// Now when we try to use JSInterop, it will throw an exception
				await Assert.ThrowsAsync<InvalidOperationException>(async () =>
				{
					await jsInterop.UpdateControlDiv("this will fail!");
				});
			});
		}

		public static class TestStaticFilesContents
		{
			public static readonly string DefaultMauiIndexHtmlContent = @"
<!DOCTYPE html>
<html>
<head testhtmlloaded=""true"">
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor app</title>
    <base href=""/"" />
</head>

<body>
	This test HTML is coming from a custom provider!
    <div id=""app""></div>

    <div id=""blazor-error-ui"">
        An unhandled error has occurred.
        <a href="""" class=""reload"">Reload</a>
        <a class=""dismiss"">🗙</a>
    </div>
    <script src=""_framework/blazor.webview.js"" autostart=""false""></script>

</body>

</html>
";
		}

		private sealed class BlazorWebViewWithCustomFiles : BlazorWebView
		{
			public Dictionary<string, string> CustomFiles { get; set; }

			public override IFileProvider CreateFileProvider(string contentRootDir)
			{
				if (CustomFiles == null)
				{
					return null;
				}
				var inMemoryFiles = new InMemoryStaticFileProvider(
					fileContentsMap: CustomFiles,
					// The contentRoot is ignored here because in WinForms it would include the absolute physical path to the app's content, which this provider doesn't care about
					contentRoot: null);

				var baseFileProvider = base.CreateFileProvider(contentRootDir);

				return baseFileProvider == null
					? inMemoryFiles
					: new CompositeFileProvider(inMemoryFiles, baseFileProvider);
			}
		}
	}
}
