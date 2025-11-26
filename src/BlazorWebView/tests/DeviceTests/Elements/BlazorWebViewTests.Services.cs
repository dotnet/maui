using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using WebViewAppShared;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
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
}
