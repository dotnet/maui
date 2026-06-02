using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if ANDROID
	/// <summary>
	/// Verifies that BlazorWebViewHandler registers an OnBackPressed lifecycle event handler
	/// when connected on Android. This handler is essential for proper back navigation within
	/// the BlazorWebView on Android 13+ with predictive back gestures.
	/// See: https://github.com/dotnet/maui/issues/32767
	/// </summary>
	[Fact]
	public async Task BlazorWebViewRegistersOnBackPressedHandler()
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
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(MauiBlazorWebView.DeviceTests.Components.NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// Get the lifecycle event service and verify OnBackPressed handler is registered
			var lifecycleService = MauiContext.Services.GetService<ILifecycleEventService>() as LifecycleEventService;
			Assert.NotNull(lifecycleService);

			// Verify the OnBackPressed event has been registered
			Assert.True(lifecycleService.ContainsEvent(nameof(AndroidLifecycle.OnBackPressed)),
				"BlazorWebViewHandler should register an OnBackPressed lifecycle event handler on Android");
		});
	}

	/// <summary>
	/// Verifies that BlazorWebViewHandler properly cleans up the OnBackPressed lifecycle event handler
	/// when disconnected. This prevents memory leaks and ensures proper cleanup.
	/// See: https://github.com/dotnet/maui/issues/32767
	/// </summary>
	[Fact]
	public async Task BlazorWebViewCleansUpOnBackPressedHandlerOnDisconnect()
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
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(MauiBlazorWebView.DeviceTests.Components.NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var lifecycleService = MauiContext.Services.GetService<ILifecycleEventService>() as LifecycleEventService;
			Assert.NotNull(lifecycleService);

			// Verify handler is registered after connect
			Assert.True(lifecycleService.ContainsEvent(nameof(AndroidLifecycle.OnBackPressed)),
				"OnBackPressed handler should be registered after ConnectHandler");

			// Count the handlers before disconnect
			var handlersBefore = lifecycleService.GetEventDelegates<AndroidLifecycle.OnBackPressed>(nameof(AndroidLifecycle.OnBackPressed));
			int countBefore = 0;
			foreach (var _ in handlersBefore)
				countBefore++;

			// Disconnect the handler by setting the BlazorWebView's Handler to null
			// This triggers DisconnectHandler internally
			bwv.Handler = null;

			// Count the handlers after disconnect
			var handlersAfter = lifecycleService.GetEventDelegates<AndroidLifecycle.OnBackPressed>(nameof(AndroidLifecycle.OnBackPressed));
			int countAfter = 0;
			foreach (var _ in handlersAfter)
				countAfter++;

			// Verify the handler count decreased (cleanup happened)
			Assert.True(countAfter < countBefore,
				$"OnBackPressed handler should be removed after DisconnectHandler. Before: {countBefore}, After: {countAfter}");
		});
	}
#endif
}
