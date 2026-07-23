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
				"document.body.innerText.indexOf('coming from AppType') >= 0");
			Assert.Equal("true", hostMarker.Trim('"'));

			// Clicking the button in the attached component increments the counter, proving the
			// converted <TestComponent1 @rendermode> is interactive.
			await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.getElementById('incrementButton').click()");
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "1");

			var counterValue = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.getElementById('counterValue').innerText");
			Assert.Equal("1", counterValue.Trim('"'));
		});
	}
}
