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
}
