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
	public async Task RootComponentsCanBeClearedAndReaddedAfterInitialization()
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
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(TestComponent1), Selector = "#app" });

		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = CreateHandler<BlazorWebViewHandler>(bwv);
			try
			{
				var platformWebView = handler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);
				await WebViewHelpers.WaitForControlDiv(platformWebView, "0");

				for (var i = 0; i < 2; i++)
				{
					bwv.RootComponents.Clear();
					await WebViewHelpers.WaitForCondition(platformWebView,
						"document.getElementById('app').textContent.trim() === ''");

					bwv.RootComponents.Clear();
					bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(TestComponent1), Selector = "#app" });
					await WebViewHelpers.WaitForControlDiv(platformWebView, "0");
				}
			}
			finally
			{
				((IElementHandler)handler).DisconnectHandler();
			}
		});
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task RootComponentsCanBeAddedAndRemovedAfterInitialization(bool initiallyPresent)
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
				{
					"index.html",
					TestStaticFilesContents.DefaultMauiIndexHtmlContent.Replace(
						"<div id=\"app\"></div>",
						"<div id=\"app\"></div><div id=\"dynamic-app\"></div>",
						StringComparison.Ordinal)
				},
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app" });
		var component = new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#dynamic-app" };
		if (initiallyPresent)
		{
			bwv.RootComponents.Add(component);
		}

		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = CreateHandler<BlazorWebViewHandler>(bwv);
			try
			{
				var platformWebView = handler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);
				await WebViewHelpers.WaitForCondition(platformWebView,
					"document.querySelector('#app #controlDiv')?.innerText === 'Static'");
				if (initiallyPresent)
				{
					await WebViewHelpers.WaitForCondition(platformWebView,
						"document.querySelector('#dynamic-app #controlDiv')?.innerText === 'Static'");
				}

				for (var i = 0; i < 4; i++)
				{
					if (bwv.RootComponents.Contains(component))
					{
						bwv.RootComponents.Remove(component);
						await WebViewHelpers.WaitForCondition(platformWebView,
							"document.getElementById('dynamic-app').textContent.trim() === ''");
					}
					else
					{
						bwv.RootComponents.Add(component);
						await WebViewHelpers.WaitForCondition(platformWebView,
							"document.querySelector('#dynamic-app #controlDiv')?.innerText === 'Static'");
					}
				}
			}
			finally
			{
				((IElementHandler)handler).DisconnectHandler();
			}
		});
	}

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
