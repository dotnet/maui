using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
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

	[Fact]
	public void UsePlatformHandlerGenericReplacesDefaultBlazorWebViewHandler()
	{
		// Verifies that the public UsePlatformHandler<THandler>() extension on IMauiBlazorWebViewBuilder
		// actually replaces the default BlazorWebViewHandler registered by AddMauiBlazorWebView()
		// for the IBlazorWebView service type. The two HostBuilderHandlerTests in Core verify the
		// underlying ConfigureMauiHandlers replacement mechanism with stub types; this test exercises
		// the new public API surface end-to-end with the real BlazorWebView/IBlazorWebView types.
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddMauiBlazorWebView()
			.UsePlatformHandler<CustomBlazorWebViewHandlerStub>();
		using var app = builder.Build();

		var handlersFactory = app.Services.GetRequiredService<IMauiHandlersFactory>();
		Assert.Equal(typeof(CustomBlazorWebViewHandlerStub), handlersFactory.GetHandlerType(typeof(BlazorWebView)));
	}

	[Fact]
	public void UsePlatformHandlerFactoryReplacesDefaultBlazorWebViewHandler()
	{
		// Companion to UsePlatformHandlerGenericReplacesDefaultBlazorWebViewHandler — verifies the
		// factory overload (Func<IServiceProvider, IViewHandler>) also replaces the default handler.
		// The factory overload registers a different ServiceDescriptor shape (ImplementationFactory
		// rather than ImplementationType), so GetHandlerType returns null here; we resolve through
		// GetHandler instead and assert the produced instance type.
		var builder = MauiApp.CreateBuilder();
		var factoryWasCalled = false;
		builder.Services.AddMauiBlazorWebView()
			.UsePlatformHandler(_ =>
			{
				factoryWasCalled = true;
				return new CustomBlazorWebViewHandlerStub();
			});
		using var app = builder.Build();

		var handlersFactory = app.Services.GetRequiredService<IMauiHandlersFactory>();
		var handler = handlersFactory.GetHandler(typeof(BlazorWebView));

		Assert.True(factoryWasCalled, "Factory delegate should have been invoked when the handler was resolved.");
		Assert.IsType<CustomBlazorWebViewHandlerStub>(handler);
	}

	private class CustomBlazorWebViewHandlerStub : BlazorWebViewHandler
	{
		// Marker subclass used only to prove that UsePlatformHandler replaced the default
		// BlazorWebViewHandler registration. Inheriting from BlazorWebViewHandler keeps the
		// IViewHandler contract honored on every device-test target framework without forcing
		// us to reimplement the full handler surface.
		public CustomBlazorWebViewHandlerStub() { }
	}
}
