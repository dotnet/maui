#if MACCATALYST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public async Task HighFrequencyUpdates_DoNotGrowRuntimeDelegateHandles()
	{
		var webView = await CreateCounterWebViewAsync();

		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = CreateHandler<BlazorWebViewHandler>(webView);
			await WebViewHelpers.WaitForWebViewReady(handler.PlatformView);
			await WebViewHelpers.WaitForControlDiv(handler.PlatformView, "0");

			ForceFullGc();
			var handlesBefore = GetRuntimeDelegateHandleCount();

			const int clickCount = 200;
			await WebViewHelpers.ExecuteScriptAsync(handler.PlatformView,
				$"(function() {{ for (let i = 0; i < {clickCount}; i++) document.getElementById('incrementButton').click(); return true; }})()");
			await WebViewHelpers.WaitForControlDiv(handler.PlatformView, clickCount.ToString());

			ForceFullGc();
			var handlesAfter = GetRuntimeDelegateHandleCount();
			var growth = handlesAfter - handlesBefore;

			Assert.True(growth <= 20,
				$"Runtime delegate handle count grew by {growth} after {clickCount} updates (before={handlesBefore}, after={handlesAfter}).");
		});
	}

	[Fact]
	public async Task IdleBlazorWebView_DoesNotGrowRuntimeDelegateHandles()
	{
		var webView = await CreateCounterWebViewAsync();

		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = CreateHandler<BlazorWebViewHandler>(webView);
			await WebViewHelpers.WaitForWebViewReady(handler.PlatformView);
			await WebViewHelpers.WaitForControlDiv(handler.PlatformView, "0");

			ForceFullGc();
			var handlesBefore = GetRuntimeDelegateHandleCount();

			await Task.Delay(1000);

			ForceFullGc();
			var handlesAfter = GetRuntimeDelegateHandleCount();
			var growth = handlesAfter - handlesBefore;

			Assert.True(growth <= 5,
				$"Runtime delegate handle count grew by {growth} while idle (before={handlesBefore}, after={handlesAfter}).");
		});
	}

	[Fact]
	public async Task HighFrequencyUpdates_StillUpdateCounterCorrectly()
	{
		var webView = await CreateCounterWebViewAsync();

		await InvokeOnMainThreadAsync(async () =>
		{
			var handler = CreateHandler<BlazorWebViewHandler>(webView);
			await WebViewHelpers.WaitForWebViewReady(handler.PlatformView);
			await WebViewHelpers.WaitForControlDiv(handler.PlatformView, "0");

			const int clickCount = 50;
			await WebViewHelpers.ExecuteScriptAsync(handler.PlatformView,
				$"(function() {{ for (let i = 0; i < {clickCount}; i++) document.getElementById('incrementButton').click(); return true; }})()");
			await WebViewHelpers.WaitForControlDiv(handler.PlatformView, clickCount.ToString());

			var finalCounterValue = await WebViewHelpers.ExecuteScriptAsync(handler.PlatformView, "document.getElementById('counterValue').innerText");
			finalCounterValue = finalCounterValue.Trim('"');

			Assert.Equal(clickCount.ToString(), finalCounterValue);
		});
	}

	private void EnsureBlazorHandlerCreated()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});
	}

	private Task<BlazorWebViewWithCustomFiles> CreateCounterWebViewAsync()
	{
		EnsureBlazorHandlerCreated();

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};

		bwv.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(MauiBlazorWebView.DeviceTests.Components.TestComponent1),
			Selector = "#app",
		});

		return Task.FromResult(bwv);
	}

	private static int GetRuntimeDelegateHandleCount()
	{
		var runtimeType = typeof(NSObject).Assembly.GetType("ObjCRuntime.Runtime");
		Assert.NotNull(runtimeType);

		var fields = runtimeType!
			.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
			.Where(field => IsIntPtrToGcHandleDictionary(field.FieldType))
			.ToArray();

		Assert.NotEmpty(fields);

		var total = 0;
		foreach (var field in fields)
		{
			if (field.GetValue(null) is IDictionary dictionary)
			{
				total += dictionary.Count;
			}
		}

		return total;
	}

	private static bool IsIntPtrToGcHandleDictionary(Type fieldType)
	{
		if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
		{
			return false;
		}

		var genericArguments = fieldType.GetGenericArguments();
		return genericArguments.Length == 2 &&
			genericArguments[0] == typeof(IntPtr) &&
			genericArguments[1] == typeof(GCHandle);
	}

	private static void ForceFullGc()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}

#endif
