using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public async Task AppTypeRendersHostDocumentAndAttachesComponent()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		// No HostPage and no RootComponents: AppType renders the full document (its <head>, static
		// markup and script) and converts the interactive <TestComponent1 @rendermode> into a mount
		// element at #app plus an attach registration.
		var bwv = new BlazorWebView
		{
			AppType = typeof(TestHostApp),
		};

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// The interactive component rendered by AppType should be live and start at 0.
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "0");

			// The static document authored in AppType (TestHostApp) should be the served host page.
			var hostMarker = await WebViewHelpers.ExecuteScriptAsync(
				platformWebView,
				"(document.body.innerText.indexOf('coming from AppType') >= 0 ? 'yes' : 'no')");
			Assert.Equal("yes", hostMarker.Trim('"'));

			// Clicking the button in the attached component increments the counter, proving the
			// converted <TestComponent1 @rendermode> is interactive.
			await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.getElementById('incrementButton').click()");
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "1");

			var counterValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.getElementById('counterValue').innerText");
			Assert.Equal("1", counterValue.Trim('"'));
		});
	}

	[Fact]
	public async Task AppTypeSupportsDynamicHeadViaHeadOutlet()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		// The host document (TestHostAppWithHead) declares <HeadOutlet> and a component that sets
		// <PageTitle>. BlazorWebView attaches HeadOutlet at head::after, so the PageTitle should
		// update the live document title from its static initial value.
		var bwv = new BlazorWebView
		{
			AppType = typeof(TestHostAppWithHead),
		};

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// Wait for the interactive component to render.
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "ready");

			// The document started with <title>Initial Title</title>; the interactive PageTitle,
			// rendered into the attached HeadOutlet, should have updated it.
			var title = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.title");
			Assert.Equal("Updated Title", title.Trim('"'));
		});
	}

	[Fact]
	public async Task AppTypeResolvesFingerprintedAssetsViaAssets()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		// The host document references a static asset via @Assets. With the bundled manifest,
		// @Assets should resolve to the fingerprinted URL, and requesting that URL should serve the
		// physical asset file.
		var bwv = new BlazorWebView
		{
			AppType = typeof(TestHostAppWithAssets),
		};

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "0");

			// @Assets should have emitted the fingerprinted URL, not the plain asset path.
			var src = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.getElementById('assetImg').getAttribute('src')");
			src = src.Trim('"');
			Assert.Contains("_content/WebViewAppShared/background.", src, StringComparison.Ordinal);
			Assert.DoesNotContain("background.png", src, StringComparison.Ordinal); // fingerprinted, e.g. background.<hash>.png
			Assert.EndsWith(".png", src, StringComparison.Ordinal);

			// The fingerprinted URL must actually serve the physical asset: the image should load
			// (non-zero natural size proves real image bytes were served for the fingerprinted route).
			var loaded = false;
			for (var i = 0; i < 30 && !loaded; i++)
			{
				var result = await WebViewHelpers.ExecuteScriptAsync(
					platformWebView,
					"(function(){ var img = document.getElementById('assetImg'); return (img.complete && img.naturalWidth > 0) ? 'yes' : 'no'; })()");
				loaded = result.Trim('"').Equals("yes", StringComparison.OrdinalIgnoreCase);
				if (!loaded)
				{
					await Task.Delay(500);
				}
			}
			Assert.True(loaded, "The fingerprinted asset URL should serve the physical image file.");
		});
	}
}
