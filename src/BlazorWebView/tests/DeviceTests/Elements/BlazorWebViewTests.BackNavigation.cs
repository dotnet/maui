using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if ANDROID
	/// <summary>
	/// Verifies that BlazorWebViewHandler uses OnBackPressedCallback (AndroidX) for back
	/// navigation instead of IOnBackInvokedCallback, ensuring the system predictive
	/// back-to-home animation plays when the WebView has no back history.
	/// </summary>
	[Fact]
	public async Task BlazorWebViewBackCallbackDisabledWhenCannotGoBack()
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

			// After initial load with no navigation history, CanGoBack should be false,
			// so the back callback should be disabled, allowing the system animation to play
			Assert.False(platformWebView.CanGoBack(),
				"WebView should not be able to go back after initial page load");
		});
	}

	[Fact]
	public async Task BackCallbackConsumesFirstBackPressWhenStaleEnabledRepro()
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
			var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity as global::AndroidX.Activity.ComponentActivity;
			Assert.NotNull(activity);

			var lowerPriorityCallback = new RecordingBackPressedCallback();
			activity.OnBackPressedDispatcher.AddCallback(activity, lowerPriorityCallback);

			try
			{
				var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
				var platformWebView = bwvHandler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);

				Assert.False(platformWebView.CanGoBack(), "The repro needs a BlazorWebView with no WebView history.");

				var blazorBackCallback = GetRegisteredBackPressedCallback(bwvHandler);
				blazorBackCallback.Enabled = true;

				activity.OnBackPressedDispatcher.OnBackPressed();

				Assert.Equal(1, lowerPriorityCallback.InvocationCount);
				Assert.False(blazorBackCallback.Enabled);

				activity.OnBackPressedDispatcher.OnBackPressed();

				Assert.Equal(2, lowerPriorityCallback.InvocationCount);
			}
			finally
			{
				lowerPriorityCallback.Remove();
				lowerPriorityCallback.Dispose();
				bwv.Handler = null;
			}
		});
	}

	static global::AndroidX.Activity.OnBackPressedCallback GetRegisteredBackPressedCallback(BlazorWebViewHandler handler)
	{
		var field = typeof(BlazorWebViewHandler).GetField("_backPressedCallback", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);

		return Assert.IsAssignableFrom<global::AndroidX.Activity.OnBackPressedCallback>(field.GetValue(handler));
	}

	sealed class RecordingBackPressedCallback : global::AndroidX.Activity.OnBackPressedCallback
	{
		public RecordingBackPressedCallback() : base(true)
		{
		}

		public int InvocationCount { get; private set; }

		public override void HandleOnBackPressed()
		{
			InvocationCount++;
		}
	}
#endif
}
