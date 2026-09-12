#nullable enable annotations
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Graphics;
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
	public void BlazorWebViewCreateFileProviderBeforeHandlerThrowsClearException()
	{
		var exception = Assert.Throws<InvalidOperationException>(
			() => new BlazorWebView().CreateFileProvider("wwwroot"));

		Assert.Contains("must be connected to a handler", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public async Task BlazorWebViewCreateFileProviderWithIncompatibleHandlerThrowsClearException()
	{
		await InvokeOnMainThreadAsync(() =>
		{
			var handler = new ViewHandlerStub();
			var blazorWebView = new BlazorWebView();
			handler.SetVirtualView(blazorWebView);
			blazorWebView.Handler = handler;

			var exception = Assert.Throws<InvalidOperationException>(
				() => blazorWebView.CreateFileProvider("wwwroot"));

			Assert.Contains(nameof(ViewHandlerStub), exception.Message, StringComparison.Ordinal);
			Assert.Contains(nameof(IBlazorWebViewHandler), exception.Message, StringComparison.Ordinal);
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
	public async Task UsePlatformHandlerFactoryReplacesDefaultBlazorWebViewHandler()
	{
		// Companion to UsePlatformHandlerGenericReplacesDefaultBlazorWebViewHandler — verifies the
		// factory overload (Func<IServiceProvider, IBlazorWebViewHandler>) also replaces the default handler.
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
		await InvokeOnMainThreadAsync(() =>
		{
			var handler = handlersFactory.GetHandler(typeof(BlazorWebView));

			Assert.True(factoryWasCalled, "Factory delegate should have been invoked when the handler was resolved.");
			Assert.IsType<CustomBlazorWebViewHandlerStub>(handler);
		});
	}

	[Fact]
	public async Task BlazorWebViewUsesCustomHandlerOperations()
	{
		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = new CustomBlazorWebViewHandlerStub();
			var blazorWebView = new BlazorWebView();
			handler.SetVirtualView(blazorWebView);
			blazorWebView.Handler = handler;
			var workItemCalled = false;

			Assert.Same(handler.FileProvider, blazorWebView.CreateFileProvider("wwwroot"));
			Assert.True(await blazorWebView.TryDispatchAsync(_ => workItemCalled = true));
			Assert.True(workItemCalled);
		});
	}

	private class ViewHandlerStub : IViewHandler
#if ANDROID || IOS || MACCATALYST || WINDOWS
		, IPlatformViewHandler
#endif
	{
		public bool HasContainer { get; set; }

		public object? ContainerView => null;

#if ANDROID || IOS || MACCATALYST || WINDOWS
		public object? PlatformView => _platformView;
#else
		public object? PlatformView => null;
#endif

		public IView? VirtualView { get; private set; }

		IElement? IElementHandler.VirtualView => VirtualView;

		public IMauiContext? MauiContext { get; private set; }

		public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

		public void SetVirtualView(IElement view) => VirtualView = (IView)view;

		public void UpdateValue(string property) { }

		public void Invoke(string command, object? args = null) { }

		public void DisconnectHandler() { }

		public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

		public void PlatformArrange(Rect frame) { }

#if ANDROID
		private readonly global::Android.Views.View _platformView = new(global::Android.App.Application.Context);

		global::Android.Views.View IPlatformViewHandler.PlatformView => _platformView;

		global::Android.Views.View? IPlatformViewHandler.ContainerView => null;
#elif IOS || MACCATALYST
		private readonly UIKit.UIView _platformView = new();

		UIKit.UIView IPlatformViewHandler.PlatformView => _platformView;

		UIKit.UIView? IPlatformViewHandler.ContainerView => null;

		UIKit.UIViewController? IPlatformViewHandler.ViewController => null;
#elif WINDOWS
		private readonly Microsoft.UI.Xaml.FrameworkElement _platformView = new Microsoft.UI.Xaml.Controls.Grid();

		Microsoft.UI.Xaml.FrameworkElement IPlatformViewHandler.PlatformView => _platformView;

		Microsoft.UI.Xaml.FrameworkElement? IPlatformViewHandler.ContainerView => null;
#endif
	}

	private class CustomBlazorWebViewHandlerStub : ViewHandlerStub, IBlazorWebViewHandler
	{
		public IFileProvider FileProvider { get; } = new NullFileProvider();

		public IFileProvider CreateFileProvider(string contentRootDir) => FileProvider;

		public Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			workItem(EmptyServiceProvider.Instance);
			return Task.FromResult(true);
		}
	}

	private sealed class EmptyServiceProvider : IServiceProvider
	{
		public static EmptyServiceProvider Instance { get; } = new();

		public object? GetService(Type serviceType) => null;
	}
}
