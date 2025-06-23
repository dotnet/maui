using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
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
}
